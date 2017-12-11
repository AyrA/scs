using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace scs
{
    class Program
    {
#if DEBUG
        public const bool DEBUG = true;
#else
        public const bool DEBUG = false;
#endif
        static int Main(string[] args)
        {
#if DEBUG
            args = @"C:\Projects\scs\scs\Examples\Forms\drawing.cs".Split(' ');
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
#if DEBUG
            Console.Error.WriteLine("#END");
            Console.ReadKey(true);
#endif
            return 0;
        }

        private static int Run(string IN, string[] Args)
        {
            //Save Colors
            var Colors = new
            {
                FG = Console.ForegroundColor,
                BG = Console.BackgroundColor
            };
            try
            {
                return Compiler.Run(IN, Args.Length > 0 ? Args : null, !DEBUG);
            }
            catch (ScriptException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Error.WriteLine("The script '{0}' caused an Exception", ex.FileName);
                Console.Error.WriteLine(ex.InnerException);
            }
            catch (AggregateException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine("Got {0} Errors", ex.InnerExceptions.Count);
                foreach (var e in ex.InnerExceptions)
                {
                    Console.Error.WriteLine(e);
                }
            }
            catch (IOException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Error.WriteLine("Compiler File IO Error: {0}", ex.Message);
            }
            catch (DependencyException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Error.WriteLine("{1}:[{0}] {2}", ex.LineNumber, ex.FileName, ex.Message);
                if (ex.InnerException != null)
                {
                    Console.Error.WriteLine(ex.InnerException.Message);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Error.WriteLine("Unexpected Error:");
                Console.Error.WriteLine(ex);
            }
            //Reset Colors
            Console.ForegroundColor = Colors.FG;
            Console.BackgroundColor = Colors.BG;
            return int.MinValue;
        }

        private static void Compile(string IN, string OUT)
        {
            try
            {
                var Errors = Compiler.Compile(IN, OUT, !DEBUG);
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
                        if (C.Compile)
                        {
                            Console.Error.WriteLine("Duplicate /c Argument");
                            return C;
                        }
                        C.Compile = true;
                        if (i < Args.Length - 1)
                        {
                            if (string.IsNullOrEmpty(C.OutputFile))
                            {
                                C.OutputFile = Args[++i];
                            }
                            else
                            {
                                Console.Error.WriteLine("/c Argument can't be last because it requires a file name parameter.");
                                return C;
                            }
                        }
                        else
                        {
                            Console.Error.WriteLine("/c Argument requires a file name parameter");
                            return C;
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
