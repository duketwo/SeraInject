namespace CSharpLua.Structs
{
    using System;
    using System.Collections;

    public class LuaFuncDescriptor
    {
        private string FunctionDoc;
        private string FunctionDocString;
        private string FunctionName;
        private ArrayList FunctionParamDocs;
        private ArrayList FunctionParameters;

        public LuaFuncDescriptor(string strFuncName, string strFuncDoc, ArrayList strParams, ArrayList strParamDocs)
        {
            this.FunctionName = strFuncName;
            this.FunctionDoc = strFuncDoc;
            this.FunctionParameters = strParams;
            this.FunctionParamDocs = strParamDocs;
            string str = strFuncName + "(%params%) - " + strFuncDoc;
            string str2 = "";
            string newValue = "";
            bool flag = true;
            for (int i = 0; i < strParams.Count; i++)
            {
                if (!flag)
                {
                    newValue = newValue + ", ";
                }
                newValue = newValue + strParams[i];
                object obj2 = str2;
                str2 = string.Concat(new object[] { obj2, "\n\t", strParams[i], "\t\t", strParamDocs[i] });
                flag = false;
            }
            this.FunctionDocString = str.Replace("%params%", newValue) + str2;
        }

        public string getFuncDoc()
        {
            return this.FunctionDoc;
        }

        public string getFuncFullDoc()
        {
            return this.FunctionDocString;
        }

        public string getFuncHeader()
        {
            if (this.FunctionDocString.IndexOf("\n") == -1)
            {
                return this.FunctionDocString;
            }
            return this.FunctionDocString.Substring(0, this.FunctionDocString.IndexOf("\n"));
        }

        public string getFuncName()
        {
            return this.FunctionName;
        }

        public ArrayList getFuncParamDocs()
        {
            return this.FunctionParamDocs;
        }

        public ArrayList getFuncParams()
        {
            return this.FunctionParameters;
        }
    }
}

