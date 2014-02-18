namespace CSharpLua.Structs
{
    using System;

    public class AttrLuaFunc : Attribute
    {
        private string FunctionDoc;
        private string FunctionName;
        private string[] FunctionParameters;

        public AttrLuaFunc(string strFuncName, string strFuncDoc)
        {
            this.FunctionParameters = null;
            this.FunctionName = strFuncName;
            this.FunctionDoc = strFuncDoc;
        }

        public AttrLuaFunc(string strFuncName, string strFuncDoc, params string[] strParamDocs)
        {
            this.FunctionParameters = null;
            this.FunctionName = strFuncName;
            this.FunctionDoc = strFuncDoc;
            this.FunctionParameters = strParamDocs;
        }

        public string getFuncDoc()
        {
            return this.FunctionDoc;
        }

        public string getFuncName()
        {
            return this.FunctionName;
        }

        public string[] getFuncParams()
        {
            return this.FunctionParameters;
        }
    }
}

