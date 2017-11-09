using System.Linq;
using System.IO;
using Microsoft.CSharp;
using System.Reflection;
using System.Diagnostics;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

namespace scs
{
    public static class Compiler
    {
        /// <summary>
        /// Recursively Resolves Dependencies from a Source File
        /// </summary>
        /// <param name="SourceFileName">Source File Name</param>
        /// <returns>List of Dependencies</returns>
        public static ScriptDependency[] ResolveDependencies(string SourceFileName)
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
            foreach (var Line in File.ReadAllLines(SourceFileName).Select(m => m.Trim()))
            {
                ++LineNum;
                //Ignore empty Lines and simple comments
                if (!string.IsNullOrEmpty(Line) && !Line.StartsWith("//"))
                {
                    //The first non-header line stops header processing
                    if (!Line.Contains(" ") || !Line.StartsWith("#"))
                    {
                        return;
                    }
                    var Command = Line.Substring(0, Line.IndexOf(' ')).Trim().ToLower();
                    var FileName = Line.Substring(Line.IndexOf(' ') + 1).Trim();
                    //The File name must be a quoted string
                    if (FileName.StartsWith("\"") && Line.EndsWith("\""))
                    {
                        //Remove Quotes
                        FileName = FileName.Substring(0, FileName.Length - 2);
                        //Make Absolute Path
                        FileName = Tools.GetFullName(Path.Combine(Path.GetDirectoryName(SourceFileName), FileName));

                        switch (Command)
                        {
                            case "#include":
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
                                            break;
                                        default:
                                            throw new DependencyException(LineNum, SourceFileName, "Error processing # include statement: The referenced file '{FileName}' is not a valid Script Binary");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    throw new DependencyException(LineNum, SourceFileName, "Error processing #include Statement", ex);
                                }
                                break;
                            case "#ref":
                                if (ExistingDependencies.All(m => m.Path != FileName))
                                {
                                    ExistingDependencies.Add(new ScriptDependency()
                                    {
                                        Path = FileName,
                                        Type = ScriptDependencyType.Library
                                    });
                                }
                                break;
                            default:
                                //Stop Processing References and Includes once we get an unsupported Line.
                                return;
                        }
                    }
                }

            }
        }

        /// <summary>
        /// Gets the Script type (Binary or Source) from a File
        /// </summary>
        /// <remarks>This will not validate if the file is a valid Source or Binary</remarks>
        /// <param name="FileName">File Name</param>
        /// <returns>Script Type</returns>
        public static ScriptDependencyType GetScriptType(string FileName)
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
        /// Compiles one or many Source Files into a binary Script
        /// </summary>
        /// <param name="SourceFiles"></param>
        /// <param name="OutFile"></param>
        public static void Compile(string[] SourceFiles, string OutFile)
        {
            var codeProvider = new CSharpCodeProvider();
            var compilerParams = new CompilerParameters();
            compilerParams.CompilerOptions = "/target:library /optimize";
            compilerParams.GenerateExecutable = false;
            compilerParams.GenerateInMemory = false;
            compilerParams.IncludeDebugInformation = false;
            compilerParams.ReferencedAssemblies.Add("mscorlib.dll");
            compilerParams.ReferencedAssemblies.Add("System.dll");
            compilerParams.ReferencedAssemblies.Add(Path.Combine(Tools.EnginePath, "Engine.dll"));
        }
    }

    public struct ScriptDependency
    {
        public ScriptDependencyType Type;
        public string Path;
    }

    public enum ScriptDependencyType : int
    {
        Unknown = 0,
        Library = 1,
        ScriptFile = 2,
        ScriptBinary = 3,
        Invalid = int.MaxValue
    }
}
