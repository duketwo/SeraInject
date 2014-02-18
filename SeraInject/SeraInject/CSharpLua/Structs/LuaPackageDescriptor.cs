namespace CSharpLua.Structs
{
    using System;
    using System.Collections;

    public class LuaPackageDescriptor
    {
        public string PackageDoc;
        public Hashtable PackageFuncs;
        public string PackageName;

        public LuaPackageDescriptor(string strName, string strDoc)
        {
            this.PackageName = strName;
            this.PackageDoc = strDoc;
        }

        public void AddFunc(LuaFuncDescriptor pFunc)
        {
            if (this.PackageFuncs == null)
            {
                this.PackageFuncs = new Hashtable();
            }
            this.PackageFuncs.Add(pFunc.getFuncName(), pFunc);
        }

        public string getPackageDoc()
        {
            return this.PackageDoc;
        }

        public string getPackageName()
        {
            return this.PackageName;
        }

        public bool HasFunc(string strFunc)
        {
            return this.PackageFuncs.ContainsKey(strFunc);
        }

        public void WriteHelp()
        {
            Console.WriteLine("Available commands on " + this.PackageName + ": ");
            Console.WriteLine();
            IDictionaryEnumerator enumerator = this.PackageFuncs.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Console.WriteLine(((LuaFuncDescriptor)enumerator.Value).getFuncHeader());
            }
        }

        public void WriteHelp(string strCmd)
        {
            Console.WriteLine(((LuaFuncDescriptor)this.PackageFuncs[strCmd]).getFuncFullDoc());
        }
    }
}

