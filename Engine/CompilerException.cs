using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;

namespace scs
{
    public class CompilerException : Exception
    {
        private Dictionary<string, string> _data;

        public CompilerError Result
        { get; private set; }

        public override string Message
        {
            get
            {
                return "A Compiler Error occured: " + ToString();
            }
        }

        public override IDictionary Data
        {
            get
            {
                return new Dictionary<string, string>(_data);
            }
        }

        public CompilerException(CompilerError E)
        {
            Result = E;
            _data = new Dictionary<string, string>();
            _data.Add("Line", E.Line.ToString());
            _data.Add("Column", E.Column.ToString());
            _data.Add("File", E.FileName);
            _data.Add("Code", E.ErrorNumber);
            _data.Add("Message", E.ErrorText);
        }

        public override string ToString()
        {
            return $"{Result.FileName} [{Result.Line}:{Result.Column}] {Result.ErrorNumber}: {Result.ErrorText}";
        }

    }
}
