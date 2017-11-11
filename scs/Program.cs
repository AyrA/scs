using System;
using System.IO;

namespace scs
{
    class Program
    {
        static int Main(string[] args)
        {
#if DEBUG
            try
            {
                Compiler.Run(@"C:\temp\test.cs", args);
            }
            catch (AggregateException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Got {0} Errors", ex.InnerExceptions.Count);
                foreach (var e in ex.InnerExceptions)
                {
                    Console.Error.WriteLine(e);
                }
            }
            catch (IOException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("IO Exception: {0}", ex.Message);
            }
            catch (DependencyException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("{1}:[{0}] {2}", ex.LineNumber, ex.FileName, ex.Message);
                if (ex.InnerException != null)
                {
                    Console.Error.WriteLine(ex.InnerException.Message);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(ex);
            }
            Console.ResetColor();
            Console.Error.WriteLine("#END");
            Console.ReadKey(true);
#else
            return 0;
#endif
        }

        public static void Help()
        {
            Console.Error.WriteLine(@"scs [/c <filename>] <script> [script params]
C# Scripting Engine

/c filename    - Compile Script file into Binary instead of executing it.
script         - Script File/Binary to execute
script params  - Paramters passed on to the script. Ignored when /c i used.");
        }
    }
}
