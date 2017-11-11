using System;
using System.IO;

namespace scs
{
    class Program
    {
        static void Main(string[] args)
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
#endif
        }

        public static void Help()
        {
            Console.Error.WriteLine("scs [/c <output>] <script> [script params]");
        }
    }
}
