using System;

namespace scs
{
    public class ScriptException : Exception
    {
        /// <summary>
        /// Gets the Script File Name
        /// </summary>
        public string FileName
        {
            get;private set;
        }

        /// <summary>
        /// Gets the <see cref="Exception.Message"/> of the InnerException
        /// </summary>
        public override string Message
        {
            get
            {
                return InnerException.Message;
            }
        }

        public ScriptException(string FileName, Exception InnerException) : base(InnerException.Message, InnerException)
        {
            this.FileName = FileName;
        }

        public override string ToString()
        {
            return $"Uncaught Script Exception in {FileName}";
        }
    }
}
