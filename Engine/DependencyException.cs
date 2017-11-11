using System;

namespace scs
{
    public class DependencyException : Exception
    {
        public int LineNumber { get; private set; }
        public string FileName { get; private set; }

        public DependencyException(int LineNumber, string FileName, string Message) : this(LineNumber, FileName, Message, null)
        {

        }

        public DependencyException(int LineNumber, string FileName, string Message, Exception InnerException) : base(Message, InnerException)
        {
            this.LineNumber = LineNumber;
            this.FileName = FileName;
        }
    }
}
