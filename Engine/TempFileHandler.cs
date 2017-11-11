using System;
using System.IO;

namespace scs
{
    public class TempFileHandler : IDisposable
    {
        public string TempName
        { get; private set; }

        public bool Exists
        {
            get
            {
                return File.Exists(TempName);
            }
        }

        public TempFileHandler() : this(Path.GetTempFileName())
        {
        }

        public TempFileHandler(string TempFileName)
        {
            TempName = TempFileName;
        }

        ~TempFileHandler()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (Exists)
            {
                try
                {
                    File.Delete(TempName);
                }
                catch
                {

                }
            }
        }
    }
}
