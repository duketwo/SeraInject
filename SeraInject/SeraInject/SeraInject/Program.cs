namespace SeraInject
{
    using CSharpLua.Structs;
    using LuaInterface;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Media;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using Utilities.IniControl;
    using Utilities.MemoryHandling;

    public class Program
    {
        private static double SeraInjectVersion = 0.21;
        private bool bRunning = true;
        private static StreamWriter history;
        private IniReader ini;
        private ReadWriteMemory mem;
        private static Dictionary<string, LuaFuncDescriptor> pLuaFuncs = null;
        private static Hashtable pLuaPackages = null;
        private static Lua pLuaVM = null;

        public Program()
        {
            pLuaVM = new Lua();
            pLuaFuncs = new Dictionary<string, LuaFuncDescriptor>();
            pLuaPackages = new Hashtable();
            registerLuaFunctions(null, this, null);
            Console.WriteLine("Sera Inject v" + SeraInjectVersion + " - CSharp Lua Console ");
            Console.Title = "Sera Inject " + SeraInjectVersion;
            this.checkVersion();
            this.setHistory();
        }

        [AttrLuaFunc("beep", "Creates a tone at the given frequency for the given amount of time", new string[] { "Frequency in hertz", "Duration in milliseconds" })]
        public void beep(int frequency, int duration)
        {
            Console.Beep(frequency, duration);
        }

        private void checkVersion()
        {
            this.SettingsOpen("SeraInject.ini");
            this.SettingsSection("Settings");
            string str = this.SettingsRead("Version");
            if (str == "")
            {
                this.SettingsWrite("Version", SeraInjectVersion.ToString());
            }
            else if (!((SeraInjectVersion <= Convert.ToDouble(str)) && File.Exists("SeraInject Help.txt")))
            {
                this.SettingsWrite("Version", SeraInjectVersion.ToString());
                this.updateHelp();
            }
        }

        [AttrLuaFunc("cleanPatternFile", "Removes duplicate entries from a pattern file", new string[] { "File Location" })]
        public void cleanPatternFile(string filename)
        {
            string str;
            Hashtable hashtable = new Hashtable();
            TextReader reader = new StreamReader(filename);
            while ((str = reader.ReadLine()) != null)
            {
                if (!hashtable.ContainsKey(str))
                {
                    hashtable.Add(str, "");
                }
            }
            reader.Close();
            TextWriter writer = new StreamWriter(filename);
            foreach (string str2 in hashtable.Keys)
            {
                writer.WriteLine(str2);
            }
            writer.Close();
        }

        [AttrLuaFunc("clr", "Aliases cls() - Clears the screen.")]
        public void clr()
        {
            this.cls();
        }

        [AttrLuaFunc("cls", "Clears the screen.")]
        public void cls()
        {
            Console.Clear();
        }

        [AttrLuaFunc("CreatePatternMaskFromFile", "Takes a file and creates a pattern mask according to each line in the file.", new string[] { "Location to file that contains at least two lines of patterns" })]
        public string CreatePatternMaskFromFile(string fileLocation)
        {
            StreamReader reader = new StreamReader(fileLocation);
            int length = 0;
            bool flag = true;
            string[] strArray = new string[0];
            char[] chArray = new char[0];
            string[] strArray2 = new string[0];
            while (!reader.EndOfStream)
            {
                string str;
                int num2;
                if (flag)
                {
                    str = reader.ReadLine();
                    if (str == null)
                    {
                        throw new Exception("Can not create mask from empty file: " + fileLocation);
                    }
                    length = str.Length;
                    strArray = str.Split(new char[] { ' ' });
                    chArray = new char[strArray.Length];
                    num2 = 0;
                    while (num2 < strArray.Length)
                    {
                        chArray[num2] = 'x';
                        num2++;
                    }
                    str = reader.ReadLine();
                    if (str != null)
                    {
                        strArray2 = str.Split(new char[] { ' ' });
                    }
                    else
                    {
                        return new string(chArray);
                    }
                    flag = false;
                }
                else
                {
                    str = reader.ReadLine();
                    strArray2 = str.Split(new char[] { ' ' });
                }
                if (str.Length != length)
                {
                    Console.WriteLine("Pattern lines need to be the same length of characters!");
                    return "Pattern lines need to be the same length of characters!";
                }
                for (num2 = 0; num2 < strArray.Length; num2++)
                {
                    if ((chArray[num2] == 'x') && (strArray[num2] != strArray2[num2]))
                    {
                        chArray[num2] = '?';
                    }
                }
                strArray = strArray2;
            }
            reader.Close();
            return new string(chArray);
        }

        [AttrLuaFunc("FindPattern", "Finds a pattern or signature inside of the given Process and memory range", new string[] { "Address on which the search will start.", "Address on which the search will end.", "A hexadecimal string representing the pattern to be found. Ex: \"4C 61 F3\"" })]
        public uint FindPattern(uint dwStart, uint dwEnd, string szPattern)
        {
            int num;
            if (this.mem == null)
            {
                throw new Exception("SetMemoryReader must be ran before you can find a pattern in memory");
            }
            string[] strArray = szPattern.Split(new char[] { ' ' });
            byte[] bPattern = new byte[strArray.Length];
            for (num = 0; num < strArray.Length; num++)
            {
                bPattern[num] = Convert.ToByte(strArray[num], 0x10);
            }
            string szMask = "";
            for (num = 0; num < strArray.Length; num++)
            {
                szMask = szMask + "x";
            }
            return this.mem.FindPattern(dwStart, dwEnd, bPattern, szMask);
        }

        [AttrLuaFunc("FindPatternMask", "Finds a pattern / signature inside of the given Process and memory range", new string[] { "Address on which the search will start.", "Address on which the search will end.", "A hexadecimal string representing the pattern to be found. Ex: \"4C 61 F3\"", "A string of 'x' (match), '!' (not-match), or '?' (wildcard)." })]
        public uint FindPatternMask(uint dwStart, uint dwEnd, string szPattern, string szMask)
        {
            if (this.mem == null)
            {
                throw new Exception("SetMemoryReader must be ran before you can find a pattern in memory");
            }
            string[] strArray = szPattern.Split(new char[] { ' ' });
            byte[] bPattern = new byte[strArray.Length];
            for (int i = 0; i < strArray.Length; i++)
            {
                bPattern[i] = Convert.ToByte(strArray[i], 0x10);
            }
            return this.mem.FindPattern(dwStart, dwEnd, bPattern, szMask);
        }

        [AttrLuaFunc("FindPatternMaskRetry", "Tries and continues to try to find a pattern / signature inside of the given Process and memory range", new string[] { "Address on which the search will start.", "Address on which the search will end.", "A hexadecimal string representing the pattern to be found. Ex: \"4C 61 F3\"", "A string of 'x' (match), '!' (not-match), or '?' (wildcard)." })]
        public uint FindPatternMaskRetry(uint dwStart, uint dwEnd, string szPattern, string szMask)
        {
            if (this.mem == null)
            {
                throw new Exception("SetMemoryReader must be ran before you can find a pattern in memory");
            }
            string[] strArray = szPattern.Split(new char[] { ' ' });
            byte[] bPattern = new byte[strArray.Length];
            for (int i = 0; i < strArray.Length; i++)
            {
                bPattern[i] = Convert.ToByte(strArray[i], 0x10);
            }
            return this.mem.FindPatternRetry(dwStart, dwEnd, bPattern, szMask);
        }

        [AttrLuaFunc("GetProcessID", "Returns the ID to the first process found by specified process name.", new string[] { "Process' name without .exe" })]
        public int GetProcessID(string processName)
        {
            Process[] processesByName = Process.GetProcessesByName(processName);
            if (processesByName.Length == 0)
            {
                return -1;
            }
            return processesByName[0].Id;
        }

        [AttrLuaFunc("help", "List available commands.")]
        public void help()
        {
            Console.WriteLine("Available commands: ");
            Console.WriteLine();
            IDictionaryEnumerator enumerator = pLuaFuncs.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Console.WriteLine(((LuaFuncDescriptor)enumerator.Value).getFuncHeader());
            }
            if (pLuaPackages.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Available packages: ");
                IDictionaryEnumerator enumerator2 = pLuaPackages.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    Console.WriteLine((string)enumerator2.Key);
                }
            }
        }

        [AttrLuaFunc("helpcmd", "Show help for a given command or package", new string[] { "Command / Package to get help of." })]
        public void help(string strCmd)
        {
            if (pLuaFuncs.ContainsKey(strCmd))
            {
                Console.WriteLine(pLuaFuncs[strCmd].getFuncFullDoc());
            }
            else if (strCmd.IndexOf(".") == -1)
            {
                if (pLuaPackages.ContainsKey(strCmd))
                {
                    ((LuaPackageDescriptor)pLuaPackages[strCmd]).WriteHelp();
                }
                else
                {
                    Console.WriteLine("No such function or package: " + strCmd);
                }
            }
            else
            {
                string[] strArray = strCmd.Split(new char[] { '.' });
                if (!pLuaPackages.ContainsKey(strArray[0]))
                {
                    Console.WriteLine("No such function or package: " + strCmd);
                }
                else
                {
                    LuaPackageDescriptor descriptor3 = (LuaPackageDescriptor)pLuaPackages[strArray[0]];
                    if (!descriptor3.HasFunc(strArray[1]))
                    {
                        Console.WriteLine("Package " + strArray[0] + " doesn't have a " + strArray[1] + " function.");
                    }
                    else
                    {
                        descriptor3.WriteHelp(strArray[1]);
                    }
                }
            }
        }

        [AttrLuaFunc("IntToHex", "Converts an integer into it's equivalent hex", new string[] { "Memory Location To Read" })]
        public string IntToHex(int integer)
        {
            string str = integer.ToString("X");
            return ("0x" + str);
        }

        private static void Main(string[] args)
        {
            Program program = new Program();
            if (args.Length > 0)
            {
                try
                {
                    pLuaVM.DoFile(args[0]);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    Console.ReadKey();
                }
            }
            else
            {
                program.run();
            }
        }

        [AttrLuaFunc("newVM", "Restarts the LuaVM instance")]
        public void newVM()
        {
            Console.Clear();
            pLuaVM = new Lua();
            pLuaFuncs = new Dictionary<string, LuaFuncDescriptor>();
            pLuaPackages = new Hashtable();
            registerLuaFunctions(null, this, null);
            Console.WriteLine("CSharp Lua Console v" + SeraInjectVersion);
        }

        [AttrLuaFunc("playSound", "Plays the .wav sound file from the given location", new string[] { "File Location" })]
        public void playSound(string location)
        {
            new SoundPlayer(location).Play();
        }

        [AttrLuaFunc("ProcessVersionByID", "Returns the FileVersion to the first process found by specified process ID.", new string[] { "Process' ID" })]
        public string ProcessVersionByID(int processID)
        {
            return Process.GetProcessById(processID).MainModule.FileVersionInfo.FileVersion;
        }

        [AttrLuaFunc("ProcessVersionByName", "Returns the FileVersion to the first process found by specified process name.", new string[] { "Process' name without .exe" })]
        public string ProcessVersionByName(string processName)
        {
            Process[] processesByName = Process.GetProcessesByName(processName);
            if (processesByName.Length > 0)
            {
                return processesByName[0].MainModule.FileVersionInfo.FileVersion;
            }
            return "process not found";
        }

        [AttrLuaFunc("quit", "Exit the program.")]
        public void quit()
        {
            this.bRunning = false;
        }

        [AttrLuaFunc("readline", "Waits for the user to input text and hit return and then returns their entered text.")]
        public string readline()
        {
            return Console.ReadLine();
        }

        [AttrLuaFunc("ReadMemoryAsHex", "Reads the given location in the memory as hex of the given length", new string[] { "Memory Location To Read", "Length of bytes to read" })]
        public string ReadMemoryAsHex(uint location, int length)
        {
            byte[] buffer;
            if (this.mem == null)
            {
                throw new Exception("SetMemoryReader must be ran before you can read memory");
            }
            this.mem.ReadMemory(location, length, out buffer);
            StringBuilder builder = new StringBuilder();
            string str = "";
            int num = 0;
            foreach (byte num2 in buffer)
            {
                str = num2.ToString("X");
                if (num2 < 0x10)
                {
                    builder.Append("0" + str + " ");
                }
                else
                {
                    builder.Append(str + " ");
                }
                num++;
            }
            return builder.ToString().Trim();
        }

        [AttrLuaFunc("ReadMemoryAsInt", "Read's the given location in memory as a 4 byte integer", new string[] { "Memory Location To Read" })]
        public int ReadMemoryAsInt(uint location)
        {
            byte[] buffer;
            if (this.mem == null)
            {
                throw new Exception("SetMemoryReader must be ran before you can read memory");
            }
            this.mem.ReadMemory(location, 4, out buffer);
            return BitConverter.ToInt32(buffer, 0);
        }

        [AttrLuaFunc("ReadMemoryAsIntRetry", "Tries and continues to try reading the given location in memory as a 4 byte integer", new string[] { "Memory Location To Read" })]
        public int ReadMemoryAsIntRetry(uint location)
        {
            byte[] buffer;
            if (this.mem == null)
            {
                throw new Exception("SetMemoryReader must be ran before you can read memory");
            }
            this.mem.ReadMemoryRetry(location, 4, out buffer);
            return BitConverter.ToInt32(buffer, 0);
        }

        [AttrLuaFunc("ReadMemoryAsString", "Reads the given location in the memory as a string of the given length", new string[] { "Memory Location To Read", "Length of bytes to read" })]
        public string ReadMemoryAsString(uint location, int length)
        {
            byte[] buffer;
            if (this.mem == null)
            {
                throw new Exception("SetMemoryReader must be ran before you can read memory");
            }
            this.mem.ReadMemory(location, length, out buffer);
            return Encoding.ASCII.GetString(buffer, 0, length);
        }

        public static void registerLuaFunctions(string strPackage, object pTarget, string strPkgDoc)
        {
            if (((pLuaVM != null) && (pLuaFuncs != null)) && (pLuaPackages != null))
            {
                LuaPackageDescriptor descriptor = null;
                if (strPackage != null)
                {
                    pLuaVM.DoString(strPackage + " = {}");
                    descriptor = new LuaPackageDescriptor(strPackage, strPkgDoc);
                }
                Type type = pTarget.GetType();
                foreach (MethodInfo info in type.GetMethods())
                {
                    foreach (Attribute attribute in Attribute.GetCustomAttributes(info))
                    {
                        if (attribute.GetType() == typeof(AttrLuaFunc))
                        {
                            AttrLuaFunc func = (AttrLuaFunc)attribute;
                            ArrayList strParams = new ArrayList();
                            ArrayList strParamDocs = new ArrayList();
                            string strFuncName = func.getFuncName();
                            string strFuncDoc = func.getFuncDoc();
                            string[] strArray = func.getFuncParams();
                            ParameterInfo[] parameters = info.GetParameters();
                            if ((strArray != null) && (parameters.Length != strArray.Length))
                            {
                                Console.WriteLine(string.Concat(new object[] { "Function ", info.Name, " (exported as ", strFuncName, ") argument number mismatch. Declared ", strArray.Length, " but requires ", parameters.Length, "." }));
                                break;
                            }
                            for (int i = 0; i < parameters.Length; i++)
                            {
                                strParams.Add(parameters[i].Name);
                                strParamDocs.Add(strArray[i]);
                            }
                            LuaFuncDescriptor pFunc = new LuaFuncDescriptor(strFuncName, strFuncDoc, strParams, strParamDocs);
                            if (descriptor != null)
                            {
                                descriptor.AddFunc(pFunc);
                                pLuaVM.RegisterFunction(strPackage + strFuncName, pTarget, info);
                                pLuaVM.DoString(strPackage + "." + strFuncName + " = " + strPackage + strFuncName);
                                pLuaVM.DoString(strPackage + strFuncName + " = nil");
                            }
                            else
                            {
                                pLuaFuncs.Add(strFuncName, pFunc);
                                pLuaVM.RegisterFunction(strFuncName, pTarget, info);
                            }
                        }
                    }
                }
                if (descriptor != null)
                {
                    pLuaPackages.Add(strPackage, descriptor);
                }
            }
        }

        public void run()
        {
            string chunk = "";
            bool flag = false;
            while (this.bRunning)
            {
                if (!flag)
                {
                    Console.Write("> ");
                }
                else
                {
                    Console.Write(": ");
                }
                string str2 = Console.ReadLine();
                if (history != null)
                {
                    history.WriteLine(str2);
                    history.Flush();
                }
                if (!(flag || !(str2 == "beginblock")))
                {
                    flag = true;
                }
                else if (flag && (str2 == "breakblock"))
                {
                    flag = false;
                    chunk = "";
                    Console.WriteLine();
                }
                else
                {
                    Exception exception;
                    if (flag && (str2 == "endblock"))
                    {
                        flag = false;
                        try
                        {
                            Console.WriteLine();
                            pLuaVM.DoString(chunk);
                            Console.WriteLine();
                        }
                        catch (Exception exception1)
                        {
                            exception = exception1;
                            Console.WriteLine(exception.Message);
                            Console.WriteLine();
                        }
                        finally
                        {
                            chunk = "";
                        }
                    }
                    else if (flag)
                    {
                        chunk = chunk + str2 + "\n";
                    }
                    else
                    {
                        try
                        {
                            Console.WriteLine();
                            pLuaVM.DoString(str2);
                            Console.WriteLine();
                        }
                        catch (Exception exception2)
                        {
                            exception = exception2;
                            Console.WriteLine(exception.Message);
                            Console.WriteLine();
                        }
                    }
                }
            }
        }

        [AttrLuaFunc("runfile", "Runs the specified Lua file.", new string[] { "File Name" })]
        public void runfile(string s)
        {
            pLuaVM.DoFile(s);
        }

        [AttrLuaFunc("runFile", "Runs the specified Lua file.", new string[] { "File Name" })]
        public void runFile(string s)
        {
            pLuaVM.DoFile(s);
        }

        private void setHistory()
        {
            if (File.Exists("history.txt") && (new FileInfo("history.txt").Length > 0x200000L))
            {
                if (File.Exists("history old.txt"))
                {
                    File.Delete("history old.txt");
                }
                File.Move("history.txt", "history old.txt");
            }
            try
            {
                history = new StreamWriter("history.txt", true);
                history.WriteLine("");
                history.WriteLine(DateTime.Now);
            }
            catch
            {
                Console.WriteLine("SERA DOES NOT RECORD HISTORY ON THIS WINDOW \n ANOTHER INSTANCE OF SERA IS ALREADY OPENED!");
            }
        }

        [AttrLuaFunc("SetMemoryReader", "Set the what process ID to read from", new string[] { "Process ID" })]
        public void SetMemoryReader(int ID)
        {
            this.mem = new ReadWriteMemory(ID);
        }

        [AttrLuaFunc("SettingsClose", "Creates or opens the given ini file.")]
        public void SettingsClose()
        {
            this.ini = null;
        }

        [AttrLuaFunc("SettingsOpen", "Creates or opens the given ini file.", new string[] { "Path to ini file" })]
        public void SettingsOpen(string fileName)
        {
            if (!(fileName.Contains("/") && fileName.Contains(@"\")))
            {
                fileName = Directory.GetCurrentDirectory() + @"\" + fileName;
            }
            this.ini = new IniReader(fileName);
        }

        [AttrLuaFunc("SettingsRead", "Reads back the given key's value as a string", new string[] { "Key Name" })]
        public string SettingsRead(string key)
        {
            return this.ini.ReadString(key);
        }

        [AttrLuaFunc("SettingsSection", "Changes the section of where the keys and values are saved to.", new string[] { "Section Name" })]
        public void SettingsSection(string section)
        {
            this.ini.Section = section;
        }

        [AttrLuaFunc("SettingsWrite", "Writes the given value to the given key.", new string[] { "Key Name", "Value to be written to the key" })]
        public void SettingsWrite(string key, string value)
        {
            this.ini.Write(key, value);
        }

        [AttrLuaFunc("sortPatternFile", "Sorts all the entries entries from a pattern file", new string[] { "File Location" })]
        public void sortPatternFile(string filename)
        {
            string str;
            List<string> list = new List<string>();
            TextReader reader = new StreamReader(filename);
            while ((str = reader.ReadLine()) != null)
            {
                list.Add(str);
            }
            reader.Close();
            list.Sort();
            TextWriter writer = new StreamWriter(filename);
            foreach (string str2 in list)
            {
                writer.WriteLine(str2);
            }
            writer.Close();
        }

        private void updateHelp()
        {
            StreamWriter writer = new StreamWriter("Sera Help.txt", false);
            writer.WriteLine("\n Sera " + SeraInjectVersion + " Help File");
            writer.WriteLine("Contains current functions and their parameters \n \n");
            foreach (KeyValuePair<string, LuaFuncDescriptor> pair in pLuaFuncs)
            {
                writer.WriteLine(pLuaFuncs[pair.Key].getFuncFullDoc() + "\n");
            }
            writer.Close();
        }

        [AttrLuaFunc("wait", "Waits the specified amount of time in milliseconds", new string[] { "Time to wait in milliseconds" })]
        public void wait(int time)
        {
            Thread.Sleep(time);
        }

        [AttrLuaFunc("waitkey", "Waits until a key is pressed")]
        public void waitkey()
        {
            Console.ReadKey();
        }

        [AttrLuaFunc("windowname", "Sets the window's name", new string[] { "New name for the console window" })]
        public void windowname(string windowName)
        {
            Console.Title = windowName;
        }

        [AttrLuaFunc("WriteMemoryAsInt", "Writes the given integer to the specified memory location", new string[] { "Memory Location To Read", "Integer to write to the memory" })]
        public bool WriteMemoryAsInt(uint location, int integer)
        {
            if (this.mem == null)
            {
                throw new Exception("SetMemoryReader must be ran before you can write memory");
            }
            byte[] bytes = BitConverter.GetBytes(integer);
            return this.mem.WriteMemory(location, bytes.Length, ref bytes);
        }
    }
}

