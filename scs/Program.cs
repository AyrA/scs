using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace scs
{
    class Program
    {
        static int Main(string[] args)
        {
#if DEBUG
            args = @"C:\temp\test.cs A B C D".Split(' ');
#endif
            string[] HelpArgs = "/?,-?,--help".Split(',');
            if (args.Length == 0 || args.Any(m => HelpArgs.Contains(m.ToLower())))
            {
                Help();
            }
            else
            {
                var C = ParseArgs(args);
                if (C.Valid)
                {
                    if (C.Compile)
                    {
                        Compile(C.ScriptFile, C.OutputFile);
                    }
                    else
                    {
#if !DEBUG
                        return
#endif
                            Run(C.ScriptFile, C.ScriptArgs);
                    }
                }
            }
            Console.ResetColor();
#if DEBUG
            Console.ReadKey(true);
#endif
            return 0;
        }

        private static int Run(string IN, string[] Args)
        {
            try
            {
                return Compiler.Run(IN, Args.Length > 0 ? Args : null);
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
                Console.Error.WriteLine("File IO Error: {0}", ex.Message);
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
                Console.Error.WriteLine("Unexpected Error:");
                Console.Error.WriteLine(ex);
            }
            return int.MinValue;
        }

        private static void Compile(string IN, string OUT)
        {
            try
            {
                var Errors = Compiler.Compile(IN, OUT);
                if (Errors != null && Errors.Length > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Error.WriteLine("Compilation completed with Warnings:");
                    foreach (var E in Errors)
                    {
                        Console.Error.WriteLine("{2}:[{0}:{1}] {3}; {4}", E.Line, E.Column, E.FileName, E.ErrorNumber, E.ErrorText);
                    }
                }
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
                Console.Error.WriteLine("File IO Error: {0}", ex.Message);
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
                Console.Error.WriteLine("Unexpected Error:");
                Console.Error.WriteLine(ex);
            }
        }

        private static Cmd ParseArgs(string[] Args)
        {
            var C = new Cmd();

            List<string> ScriptArgs = new List<string>();

            for (int i = 0; i < Args.Length; i++)
            {
                switch (Args[i].ToLower())
                {
                    case "/c":
                        if (i < Args.Length - 1)
                        {
                            C.OutputFile = Args[++i];
                        }
                        break;
                    default:
                        if (C.ScriptFile == null)
                        {
                            C.ScriptFile = Args[i];
                        }
                        else
                        {
                            ScriptArgs.Add(Args[i]);
                        }
                        break;
                }
            }
            C.ScriptArgs = ScriptArgs.ToArray();

            //Do basic arguments validation below

            //Script file is always needed
            if (C.ScriptFile == null)
            {
                Console.Error.WriteLine("Script File Argument is missing");
                return C;
            }
            //Special checks if /c is supplied
            if (C.Compile)
            {
                //Compilation and script arguments are mutually exclusive
                if (C.ScriptArgs.Length > 0)
                {
                    Console.Error.WriteLine("/c can't be used in combination with script arguments");
                    return C;
                }
                //Compilation requires an output file name
                else if (C.OutputFile == null)
                {
                    Console.Error.WriteLine("/c requires an output file name");
                    return C;
                }
            }
            //Here it is considered valid
            C.Valid = true;
            return C;
        }

        public static void Help()
        {
            Console.Error.WriteLine(@"scs [/c <filename>] <script> [script params]
C# Scripting Engine

/c filename    - Compile Script file into Binary instead of executing it.
script         - Script File/Binary to execute
script params  - Paramters passed on to the script. Ignored when /c i used.");
        }

        private struct Cmd
        {
            public bool Valid;
            public string ScriptFile;
            public string OutputFile;
            public bool Compile;
            public string[] ScriptArgs;
        }
    }
}
