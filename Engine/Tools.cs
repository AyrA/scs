using System.Diagnostics;
using System.IO;

namespace scs
{
    /// <summary>
    /// Generic Tools for Scripts
    /// </summary>
    public class Tools
    {
        private static string _Engine;

        /// <summary>
        /// Gets the Path of the Engine
        /// </summary>
        public static string EnginePath
        {
            get
            {
                return Path.GetDirectoryName(Engine);
            }
        }

        /// <summary>
        /// Gets the Executable Name of the Engine
        /// </summary>
        public static string EngineName
        {
            get
            {
                return Path.GetFileName(Engine);
            }
        }

        /// <summary>
        /// Gets the combined Path and Executable Name of the Engine
        /// </summary>
        public static string Engine
        {
            get
            {
                if (string.IsNullOrEmpty(_Engine))
                {
                    using (var P = Process.GetCurrentProcess())
                    {
                        _Engine = P.MainModule.FileName;
                    }
                }
                return _Engine;
            }
        }

        /// <summary>
        /// Tries to convert a File or Path string into a Proper Path String
        /// </summary>
        /// <param name="Name">Full or partial Path string</param>
        /// <remarks>This method will obtain proper casing of the path name if it exists</remarks>
        /// <returns>Full PAth string</returns>
        public static string GetFullName(string Name)
        {
            //Try File
            FileSystemInfo Info = new FileInfo(Name);

            if (Info.Exists)
            {
                try
                {
                    return Directory.GetFiles(((FileInfo)Info).DirectoryName, Info.Name)[0];
                }
                catch
                {
                    //It's possible to delete directories and files between the calls
                    return Path.GetFullPath(Name);
                }
            }
            //Try Directory
            Info = new DirectoryInfo(Name);
            if (Info.Exists)
            {
                try
                {
                    return Directory.GetDirectories(((DirectoryInfo)Info).Parent.FullName, Info.Name)[0];
                }
                catch
                {
                    //It's possible to delete directories and files between the calls
                    return Path.GetFullPath(Name);
                }
            }
            //Just return full Path
            //If you steal my code,
            //this is where you want to throw an exception
            //or return null if you prefer it to fail on invalid paths
            return Path.GetFullPath(Name);
        }
    }
}
