using System;

namespace scs
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
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
