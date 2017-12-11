using System.Linq;
using System.IO;
using Microsoft.CSharp;
using System.Reflection;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System;

namespace scs
{
    /// <summary>
    /// Compiles and Runs Scripts
    /// </summary>
    public static class Compiler
    {
        /// <summary>
        /// Used for Random Identifier Names
        /// </summary>
        private static Random R = new Random();

        /// <summary>
        /// Mode Directive
        /// </summary>
        public const string LINE_MODE = "//#mode";
        /// <summary>
        /// .NET Reference Directive
        /// </summary>
        public const string LINE_REF = "//#ref";
        /// <summary>
        /// Script Reference Directive
        /// </summary>
        public const string LINE_INCLUDE = "//#include";
        /// <summary>
        /// Script Version Directive
        /// </summary>
        public const string LINE_VERSION = "//#version";

        /// <summary>
        /// Default Script Mode
        /// </summary>
        private const ScriptMode DEFAULT_MODE = ScriptMode.Single;

        /// <summary>
        /// Recursively Resolves Dependencies from a Source File
        /// </summary>
        /// <param name="SourceFileName">Source File Name</param>
        /// <returns>List of Dependencies</returns>
        private static ScriptDependency[] ResolveDependencies(string SourceFileName)
        {
            List<ScriptDependency> Dependencies = new List<ScriptDependency>();
            ResolveDependencies(SourceFileName, Dependencies);
            return Dependencies.ToArray();
        }

        /// <summary>
        /// Recursively Resolves Dependencies from a Source File
        /// </summary>
        /// <param name="SourceFileName">Source File Name</param>
        /// <param name="ExistingDependencies">Existing List of Dependencies</param>
        private static void ResolveDependencies(string SourceFileName, List<ScriptDependency> ExistingDependencies)
        {
            if (string.IsNullOrEmpty(SourceFileName))
            {
                throw new IOException("Empty File Name");
            }
            if (!File.Exists(SourceFileName))
            {
                throw new IOException($"File '{SourceFileName}' not found");
            }
            int LineNum = 0;
            foreach (var Line in GetScriptHeader(SourceFileName))
            {
                ++LineNum;
                //Ignore empty Lines and simple comments
                if (!string.IsNullOrEmpty(Line) && Line.Contains(" "))
                {
                    var Command = Line.Substring(0, Line.IndexOf(' ')).Trim().ToLower();
                    var BaseFileName = Line.Substring(Line.IndexOf(' ') + 1).Trim();
                    //The File name must be a quoted string
                    if (
                        //Custom Includes
                        (BaseFileName.StartsWith("\"") && BaseFileName.EndsWith("\"")) ||
                        //System Defined Includes
                        (BaseFileName.StartsWith("<") && BaseFileName.EndsWith(">")))
                    {
                        //Make Absolute Path
                        var FileName = Tools.GetFullName(Path.Combine(BaseFileName.StartsWith("<") ? Tools.ReferencePath : Path.GetDirectoryName(SourceFileName), BaseFileName.Substring(1, BaseFileName.Length - 2)));

                        switch (Command)
                        {
                            case LINE_VERSION:
                                //This is here for forward compatibility
                                throw new DependencyException(LineNum, SourceFileName, "#version is not supported in this compiler. The compiler is outdated if the documentation states that this command is available.");
                            case LINE_MODE:
                                //This line is ignored when processing References
                                break;
                            case LINE_INCLUDE:
                                try
                                {
                                    switch (GetScriptType(FileName))
                                    {
                                        case ScriptDependencyType.ScriptBinary:
                                            //Don't add duplicates
                                            if (ExistingDependencies.All(m => m.Path != FileName))
                                            {
                                                ExistingDependencies.Add(new ScriptDependency()
                                                {
                                                    Path = FileName,
                                                    Type = ScriptDependencyType.ScriptBinary
                                                });
                                            }
                                            break;
                                        case ScriptDependencyType.ScriptFile:
                                            if (GetScriptMode(FileName) == ScriptMode.Complex)
                                            {
                                                //Don't add duplicates
                                                if (ExistingDependencies.All(m => m.Path != FileName))
                                                {
                                                    ExistingDependencies.Add(new ScriptDependency()
                                                    {
                                                        Path = FileName,
                                                        Type = ScriptDependencyType.ScriptFile
                                                    });
                                                    //Resolve further Dependencies
                                                    ResolveDependencies(FileName, ExistingDependencies);
                                                }
                                            }
                                            else
                                            {
                                                throw new DependencyException(LineNum, SourceFileName, $"Files used in #include must use complex mode. Use '#mode complex' in {FileName} to fix this");
                                            }
                                            break;
                                        default:
                                            throw new DependencyException(LineNum, SourceFileName, $"Error processing #include statement: The referenced file '{FileName}' is not a valid Binary");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    throw new DependencyException(LineNum, SourceFileName, "Error processing #include Statement: Unexpected error", ex);
                                }
                                break;
                            case LINE_REF:
                                if (ExistingDependencies.All(m => m.Path != FileName))
                                {
                                    ExistingDependencies.Add(new ScriptDependency()
                                    {
                                        //References work without a Path
                                        Path = BaseFileName.Substring(1, BaseFileName.Length - 2),
                                        Type = ScriptDependencyType.Library
                                    });
                                }
                                break;
                            default:
                                //Stop Processing References and Includes once we get an unsupported Line.
                                return;
                        }
                    }
                    else if (Command == LINE_REF || Command == LINE_INCLUDE)
                    {
                        throw new DependencyException(LineNum, SourceFileName, "Invalid Include or Reference Line. Missing quotes?");
                    }
                    else if (Command == LINE_VERSION)
                    {
                        throw new DependencyException(LineNum, SourceFileName, "Invalid version Line. Missing quotes?");
                    }
                }
            }
        }

        /// <summary>
        /// Turns a Script into Complex type
        /// </summary>
        /// <param name="SourceFileName">Source file</param>
        /// <returns>Full Script Content as Complex Type</returns>
        private static string NormalizeScript(string SourceFileName)
        {
            var Mode = GetScriptMode(SourceFileName);
            var Lines = File.ReadAllLines(SourceFileName);
            var Headers = GetScriptHeader(SourceFileName);

            switch (Mode)
            {
                case ScriptMode.Complex:
                    //Don't do anything on Complex mode
                    break;
                case ScriptMode.Simple:
                    Lines[Headers.Length] = $"public static class {GetIdentifier()}" + "{\r\n" + Lines[Headers.Length];
                    Lines[Lines.Length - 1] += "\r\n}";
                    break;
                case ScriptMode.Single:
                    Lines[Headers.Length] = $"public static class {GetIdentifier()}" + "{public static int Main(string[] args){\r\n" + Lines[Headers.Length];
                    Lines[Lines.Length - 1] += "\r\n}}";
                    break;
                default:
                    throw new Exception("Invalid Script Mode");
            }
            return string.Join("\r\n", Lines);
        }

        /// <summary>
        /// Generates an Identifier
        /// </summary>
        /// <returns>Identifier</returns>
        private static string GetIdentifier()
        {
            const string Charset = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Range(1, 10).Select(m => Charset[R.Next(Charset.Length)]).ToArray());
        }

        /// <summary>
        /// Gets the Script Headers
        /// </summary>
        /// <param name="SourceFileName">Script File</param>
        /// <returns>Script Header Lines</returns>
        private static string[] GetScriptHeader(string SourceFileName)
        {
            var Lines = new List<string>();
            if (string.IsNullOrEmpty(SourceFileName))
            {
                throw new IOException("Empty File Name");
            }
            if (!File.Exists(SourceFileName))
            {
                throw new IOException($"File '{SourceFileName}' not found");
            }
            foreach (var Line in File.ReadAllLines(SourceFileName).Select(m => m.Trim()))
            {
                //The first non-header line stops header processing
                if (Line.StartsWith("//") || Line == string.Empty || Line.StartsWith("using "))
                {
                    Lines.Add(Line);
                }
                else
                {
                    break;
                }
            }
            return Lines.ToArray();
        }

        /// <summary>
        /// Gets the Script Mode
        /// </summary>
        /// <param name="SourceFile">Script File</param>
        /// <returns>Script Mode</returns>
        private static ScriptMode GetScriptMode(string SourceFile)
        {
            var Type = ScriptDependencyType.Invalid;
            try
            {
                Type = GetScriptType(SourceFile);
            }
            catch
            {
                return ScriptMode.__None;
            }
            if (Type == ScriptDependencyType.ScriptFile)
            {
                foreach (var Line in GetScriptHeader(SourceFile))
                {
                    if (Line.StartsWith($"{LINE_MODE} "))
                    {
                        switch (Line.Split(' ')[1])
                        {
                            case "single":
                                return ScriptMode.Single;
                            case "simple":
                                return ScriptMode.Simple;
                            case "complex":
                                return ScriptMode.Complex;
                            default:
                                throw new Exception("Invalid Mode Line: " + Line);
                        }
                    }
                }
            }
            return DEFAULT_MODE;
        }

        /// <summary>
        /// Gets the Script type (Binary or Source) from a File
        /// </summary>
        /// <remarks>This will not validate if the file is a valid Source or Binary</remarks>
        /// <param name="FileName">File Name</param>
        /// <returns>Script Type</returns>
        private static ScriptDependencyType GetScriptType(string FileName)
        {
            var FI = new FileInfo(FileName);
            if (FI.Exists)
            {
                try
                {
                    var ScriptBinary = Assembly.LoadFile(FileName);
                    if (ScriptBinary.GetType("ScriptMain", false) != null)
                    {
                        return ScriptDependencyType.ScriptBinary;
                    }
                    return ScriptDependencyType.Invalid;
                }
                catch
                {
                    return ScriptDependencyType.ScriptFile;
                }
            }
            throw new FileNotFoundException("The given File was not found", FileName);
        }

        /// <summary>
        /// Compiles a script File
        /// </summary>
        /// <param name="ScriptFile">Script File</param>
        /// <param name="OutFile">Output File</param>
        /// <param name="Optimize">Optimize Code. This makes debugging harder</param>
        /// <returns>Compiler Errors</returns>
        public static CompilerError[] Compile(string ScriptFile, string OutFile, bool Optimize = true)
        {
            var Deps = ResolveDependencies(ScriptFile);
            using (var Handler = new TempFileHandler(ScriptFile + ".norm"))
            {
                File.WriteAllText(Handler.TempName, NormalizeScript(ScriptFile));
                var Refs = Deps
                    .Where(m => m.Type == ScriptDependencyType.ScriptBinary || m.Type == ScriptDependencyType.Library)
                    .Concat(Deps.Where(m => m.Type == ScriptDependencyType.Library))
                    .Select(m => m.Path)
                    .ToArray();
                var Scripts = (new string[] { Handler.TempName }).Concat(Deps
                    .Where(m => m.Type == ScriptDependencyType.ScriptFile)
                    .Select(m => m.Path))
                    .ToArray();
                return Compile(Scripts, Refs, OutFile, false);
            }
        }

        /// <summary>
        /// Compiles one or many Source Files into a binary Script
        /// </summary>
        /// <param name="SourceFiles">Source Files</param>
        /// <param name="References">References Assemblies</param>
        /// <param name="OutFile">Output File</param>
        /// <param name="Optimize">Optimize Code. This makes debugging harder</param>
        /// <returns>Compiler Errors</returns>
        private static CompilerError[] Compile(string[] SourceFiles, string[] References, string OutFile, bool Optimize)
        {
            var codeProvider = new CSharpCodeProvider();
            var compilerParams = new CompilerParameters();
            compilerParams.CompilerOptions = "/target:library" + (Optimize ? " /optimize" : "");
            compilerParams.GenerateExecutable = true;
            compilerParams.GenerateInMemory = false;
            compilerParams.IncludeDebugInformation = !Optimize;
            compilerParams.ReferencedAssemblies.Add("mscorlib.dll");
            compilerParams.ReferencedAssemblies.Add("System.dll");
            compilerParams.ReferencedAssemblies.Add("System.Core.dll");
            compilerParams.ReferencedAssemblies.Add("System.Linq.dll");
            compilerParams.OutputAssembly = OutFile;
            if (References != null && References.Length > 0)
            {
                compilerParams.ReferencedAssemblies.AddRange(References);
            }
            compilerParams.ReferencedAssemblies.Add(Path.Combine(Tools.EnginePath, "Engine.dll"));
            var Result = codeProvider.CompileAssemblyFromFile(compilerParams, SourceFiles);

            //Throw exception if there is an Error
            if (Result.Errors != null && Result.Errors.OfType<CompilerError>().Count(m => !m.IsWarning) > 0)
            {
                //Throw combined Compiler Exception
                throw new AggregateException(Result.Errors.OfType<CompilerError>().Select(m => new CompilerException(m)));
            }
            //Return Warnings
            return Result.Errors.OfType<CompilerError>().ToArray();
        }

        /// <summary>
        /// Runs a Script file or Assembly
        /// </summary>
        /// <param name="ScriptFile">Script file</param>
        /// <param name="ScriptArguments">Arguments to pass to the script</param>
        /// <returns>Result Code</returns>
        public static int Run(string ScriptFile, string[] ScriptArguments = null, bool Optimize = true)
        {
            //Simulates Real Main method Behavior
            if (ScriptArguments == null)
            {
                ScriptArguments = new string[0];
            }
            var T = GetScriptType(ScriptFile);
            Assembly Script;
            if (T == ScriptDependencyType.ScriptFile)
            {
                using (var TFH = new TempFileHandler())
                {
                    Compile(ScriptFile, TFH.TempName, Optimize);
                    Script = Assembly.LoadFile(TFH.TempName);
                }
            }
            else
            {
                Script = Assembly.LoadFile(ScriptFile);
            }

            foreach (var AssemblyType in Script.GetTypes())
            {
                //Only support classes
                if (AssemblyType.IsClass)
                {
                    //Find a static "Main" Method
                    var M = AssemblyType.GetMethod("Main", BindingFlags.Static | BindingFlags.Public);
                    if (M != null)
                    {
                        //Get Parameter Number
                        var Params = M.GetParameters();
                        if (Params.Length < 2)
                        {
                            //Run Method only if no parameters are needed or a single string array is needed.
                            if (Params.Length == 0 || Params[0].ParameterType == typeof(string[]))
                            {
                                object ret = null;
                                try
                                {
                                    ret = M.Invoke(null, Params.Length == 0 ? null : new object[] { ScriptArguments });
                                }
                                catch (TargetInvocationException ex)
                                {
                                    //Strip ex because it's unnecessary and confusing for script developers
                                    throw new ScriptException(ScriptFile, ex.InnerException);
                                }
                                catch (Exception ex)
                                {
                                    throw new ScriptException(ScriptFile, ex);
                                }
                                //Run according to return type
                                return M.ReturnType == typeof(int) ? (int)ret : 0;
                            }
                        }
                    }
                }
            }
            throw new BadImageFormatException("No suitable method signature found in the given File", ScriptFile);
        }

        /// <summary>
        /// Script Modes
        /// </summary>
        private enum ScriptMode : int
        {
            /// <summary>
            /// No Mode Specified or mode not applicable because script is not a source file
            /// </summary>
            __None = 0,
            /// <summary>
            /// Script is the Contents of "Main(string[] args){...}"
            /// </summary>
            Single = 1,
            /// <summary>
            /// Script is the Contents of a Class with a "Main" provided
            /// </summary>
            Simple = 2,
            /// <summary>
            /// Script is a complete Class with a "Main" provided
            /// </summary>
            Complex = 3
        }

        /// <summary>
        /// Represents a Script Dependency
        /// </summary>
        private struct ScriptDependency
        {
            /// <summary>
            /// Dependency Type
            /// </summary>
            public ScriptDependencyType Type;
            /// <summary>
            /// Full Script Path
            /// </summary>
            public string Path;
        }

        /// <summary>
        /// Represents a Script Dependency Type for the compiler
        /// </summary>
        private enum ScriptDependencyType : int
        {
            /// <summary>
            /// Unknown Reference Type
            /// </summary>
            Unknown = 0,
            /// <summary>
            /// .NET Library
            /// </summary>
            Library = 1,
            /// <summary>
            /// Script Source File
            /// </summary>
            ScriptFile = 2,
            /// <summary>
            /// Compiled Script
            /// </summary>
            ScriptBinary = 3,
            /// <summary>
            /// Invalid Type
            /// </summary>
            Invalid = int.MaxValue
        }
    }
}
