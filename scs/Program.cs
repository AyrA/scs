using System;

namespace scs
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            try
            {
                /*
                var Messages = Compiler.Compile(@"C:\temp\test.cs", @"C:\temp\test.dll");
                if (Messages.Length > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Error.WriteLine("Got {0} Warnings", Messages.Length);
                    foreach (var M in Messages)
                    {
                        Console.Error.WriteLine(new CompilerException(M));
                    }
                }
                //*/
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
            Console.Error.WriteLine("scs /nosig [/c <output>] <script> [script params]");
        }
    }
}
