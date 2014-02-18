namespace Utilities.MemoryHandling
{
    using System;
    using System.Runtime.InteropServices;

    public class ReadWriteMemory
    {
        private int m_lpBytesRead;
        private int m_lpBytesWrote;
        private IntPtr m_lpHandle;

        public ReadWriteMemory(int hwnd)
        {
            this.m_lpHandle = Imports.OpenProcess(Imports.ProcessAccessFlags.All, false, hwnd);
            if (this.m_lpHandle.ToInt32() == 0)
            {
                Console.WriteLine(string.Concat(new object[] { "Failed To Open Process: ", hwnd, ". Error: ", Imports.GetLastError() }), "Error");
            }
        }

        public ReadWriteMemory(IntPtr hwnd)
        {
            uint num;
            Imports.GetWindowThreadProcessId(hwnd, out num);
            this.m_lpHandle = Imports.OpenProcess(Imports.ProcessAccessFlags.All, false, (int)num);
            if (this.m_lpHandle.ToInt32() == 0)
            {
                Console.WriteLine(string.Concat(new object[] { "Failed To Open Process: ", hwnd, ". Error: ", Imports.GetLastError() }), "Error");
            }
        }

        public void CreateRemoteThread(uint address, IntPtr parameter)
        {
            Imports.CreateRemoteThread(this.m_lpHandle, IntPtr.Zero, 0, address, parameter, 0, IntPtr.Zero);
        }

        ~ReadWriteMemory()
        {
            Imports.CloseHandle(this.m_lpHandle);
        }

        public static uint FindPattern(byte[] bData, byte[] bPattern, string szMask)
        {
            if ((bData == null) || (bData.Length == 0))
            {
                throw new ArgumentNullException("bData");
            }
            if ((bPattern == null) || (bPattern.Length == 0))
            {
                throw new ArgumentNullException("bPattern");
            }
            if (szMask == string.Empty)
            {
                throw new ArgumentNullException("szMask");
            }
            if (bPattern.Length != szMask.Length)
            {
                throw new ArgumentException("Pattern and Mask lengths must be the same.");
            }
            bool flag = false;
            int length = bPattern.Length;
            int num4 = bData.Length - length;
            for (int i = 0; i < num4; i++)
            {
                flag = true;
                for (int j = 0; j < length; j++)
                {
                    if (((szMask[j] == 'x') && (bPattern[j] != bData[i + j])) || ((szMask[j] == '!') && (bPattern[j] == bData[i + j])))
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    return (uint)i;
                }
            }
            return 0;
        }

        public uint FindPattern(uint dwStart, uint dwEnd, byte[] bPattern, string szMask)
        {
            byte[] buffer;
            if (dwStart > dwEnd)
            {
                throw new ArgumentException("Start Address cannot be bigger than the End Address");
            }
            int bufferLength = (int)(dwEnd - dwStart);
            if ((bPattern == null) || (bPattern.Length == 0))
            {
                throw new ArgumentNullException("bData");
            }
            if (bPattern.Length != szMask.Length)
            {
                throw new ArgumentException("bData and szMask must be of the same size");
            }
            this.ReadMemory(dwStart, bufferLength, out buffer);
            if (buffer == null)
            {
                throw new Exception("Could not read memory in FindPattern.");
            }
            uint num2 = FindPattern(buffer, bPattern, szMask);
            if (num2 == 0)
            {
                return 0;
            }
            return (dwStart + num2);
        }

        public uint FindPatternRetry(uint dwStart, uint dwEnd, byte[] bPattern, string szMask)
        {
            byte[] buffer;
            if (dwStart > dwEnd)
            {
                throw new ArgumentException("Start Address cannot be bigger than the End Address");
            }
            int bufferLength = (int)(dwEnd - dwStart);
            if ((bPattern == null) || (bPattern.Length == 0))
            {
                throw new ArgumentNullException("bData");
            }
            if (bPattern.Length != szMask.Length)
            {
                throw new ArgumentException("bData and szMask must be of the same size");
            }
            this.ReadMemoryRetry(dwStart, bufferLength, out buffer);
            if (buffer == null)
            {
                throw new Exception("Could not read memory in FindPattern.");
            }
            uint num2 = FindPattern(buffer, bPattern, szMask);
            if (num2 == 0)
            {
                return 0;
            }
            return (dwStart + num2);
        }

        public static object RawDeserialize(byte[] rawData, int position, Type anyType)
        {
            int cb = Marshal.SizeOf(anyType);
            if (cb > rawData.Length)
            {
                return null;
            }
            IntPtr destination = Marshal.AllocHGlobal(cb);
            Marshal.Copy(rawData, position, destination, cb);
            object obj2 = Marshal.PtrToStructure(destination, anyType);
            Marshal.FreeHGlobal(destination);
            return obj2;
        }

        public static byte[] RawSerialize(object anything)
        {
            int cb = Marshal.SizeOf(anything);
            IntPtr ptr = Marshal.AllocHGlobal(cb);
            Marshal.StructureToPtr(anything, ptr, false);
            byte[] destination = new byte[cb];
            Marshal.Copy(ptr, destination, 0, cb);
            Marshal.FreeHGlobal(ptr);
            return destination;
        }

        public object ReadMemory(IntPtr address, Type type)
        {
            byte[] buffer;
            this.ReadMemory(address, Marshal.SizeOf(type), out buffer);
            return RawDeserialize(buffer, 0, type);
        }

        public object ReadMemory(uint address, Type type)
        {
            return this.ReadMemory((IntPtr)address, type);
        }

        public bool ReadMemory(int memoryLocation, int bufferLength, out byte[] lpBuffer)
        {
            lpBuffer = new byte[bufferLength];
            if (this.m_lpHandle.ToInt32() == 0)
            {
                return false;
            }
            if (!Imports.ReadProcessMemory(this.m_lpHandle, (IntPtr)memoryLocation, lpBuffer, bufferLength, out this.m_lpBytesRead))
            {
                Console.WriteLine("Failed to read from Address: {0:X}. Error: {1}", memoryLocation, Imports.GetLastError());
                return false;
            }
            return true;
        }

        public bool ReadMemory(IntPtr memoryLocation, int bufferLength, out byte[] lpBuffer)
        {
            lpBuffer = new byte[bufferLength];
            if (this.m_lpHandle.ToInt32() == 0)
            {
                return false;
            }
            if (!Imports.ReadProcessMemory(this.m_lpHandle, memoryLocation, lpBuffer, bufferLength, out this.m_lpBytesRead))
            {
                Console.WriteLine("Failed to read from Address: {0:X}. Error: {1}", memoryLocation, Imports.GetLastError());
                return false;
            }
            if (this.m_lpBytesRead != bufferLength)
            {
                Console.WriteLine("Failed to read the correct amount of bytes");
                return false;
            }
            return true;
        }

        public bool ReadMemory(uint memoryLocation, int bufferLength, out byte[] lpBuffer)
        {
            lpBuffer = new byte[bufferLength];
            if (this.m_lpHandle.ToInt32() == 0)
            {
                return false;
            }
            if (!Imports.ReadProcessMemory(this.m_lpHandle, (IntPtr)memoryLocation, lpBuffer, bufferLength, out this.m_lpBytesRead))
            {
                Console.WriteLine("Failed to read from Address: {0:X}. Error: {1}", memoryLocation, Imports.GetLastError());
                return false;
            }
            return true;
        }

        public bool ReadMemory(uint memoryLocation, int bufferLength, out uint uintOut)
        {
            uintOut = 0;
            if (this.m_lpHandle.ToInt32() == 0)
            {
                return false;
            }
            byte[] lpBuffer = new byte[4];
            if (!Imports.ReadProcessMemory(this.m_lpHandle, (IntPtr)memoryLocation, lpBuffer, bufferLength, out this.m_lpBytesRead))
            {
                Console.WriteLine("Failed to read from Address: {0:X}. Error: {1}", memoryLocation, Imports.GetLastError());
                return false;
            }
            uintOut = BitConverter.ToUInt32(lpBuffer, 0);
            return true;
        }

        public bool ReadMemoryRetry(uint memoryLocation, int bufferLength, out byte[] lpBuffer)
        {
            bool flag = false;
            lpBuffer = new byte[bufferLength];
            if (this.m_lpHandle.ToInt32() == 0)
            {
                return false;
            }
            while (!flag)
            {
                try
                {
                    Imports.ReadProcessMemory(this.m_lpHandle, (IntPtr)memoryLocation, lpBuffer, bufferLength, out this.m_lpBytesRead);
                    flag = true;
                }
                catch
                {
                }
            }
            return true;
        }

        public bool WriteMemory(IntPtr memoryLocation, int bufferLength, ref byte[] lpBuffer)
        {
            if (this.m_lpHandle.ToInt32() == 0)
            {
                return false;
            }
            if (!Imports.WriteProcessMemory(this.m_lpHandle, memoryLocation, lpBuffer, bufferLength, out this.m_lpBytesWrote))
            {
                return false;
            }
            return true;
        }

        public bool WriteMemory(uint memoryLocation, int bufferLength, ref byte[] lpBuffer)
        {
            return this.WriteMemory((IntPtr)memoryLocation, bufferLength, ref lpBuffer);
        }
    }
}

