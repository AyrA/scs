# SCS

scs is a C# Scripting Engine that allows you to execute C# scripts similar to shell scripts,
but with the full capabilities of the C# Language.

## How to use

	scs [/c <filename>] <script> [script params]
	C# Scripting Engine

	/c filename    - Compile Script file into Binary instead of executing it.
	script         - Script File/Binary to execute
	script params  - Paramters passed on to the script. Ignored when /c i used.

## Binary Scripts

The Engine can compile scripts into binaries if the `/c` argument is given.
Binary scripts execute faster because they can skip over the compilation step of regular scripts.
A binary Script is a normal .NET DLL file.

## Script File Format

Script files are more or less identical to normal Script files
and if you only need the "System.dll" Assembly you can compile them as-is if they follow the scheme of the "Complex" format.

### single

This is the basic format. It's used by default unless the script header specifies otherwise.
The script is treated as if it was the content of a function.
In a simple script, a `string[] args` is available to access script arguments passed by the user.

This format is very simple to use and feels similar to writing batch files.

Because the entire script content is treated as if it was the content of a method,
they are limited to what the language allows inside a method.
This limit can be bypassed by including other files or chosing the `simple` or `complex` format.

This format supports headers the same way the other formats do.

When executing this type of script,
the content is put inside a method named "Main" inside a class with a randomly generated name.
The Method signature requires the content to return an integer.
The return value will be used as exit code for the script engine.

### simple

This format is very similar to `single` but it is treated as if it was the content of a class.
This means for the script to execute the author must provide a `Main` method.
These 4 signatures are valid:

    public static int Main(string[] args){/*...*/}
    public static int Main(){/*...*/}
    public static void Main(string[] args){/*...*/}
    public static void Main(){/*...*/}

This format supports headers the same way the other formats do.

The name of the Class is irrelevant and only has to be valid.

This type of script provides more flexibility because it can contain multiple methods.

### complex

This type of script is fed to the compiler unchanged.
It must contain its own class definitions with a `Main` signature specified in the `simple` chapter.
The class must be public or the method will be ignored.

This type of script is the most cumbersome to create but provides the greatest level of flexibility.
It can contain multiple top-level classes.

# Script Header

Each Script has an optional Header.
A Header has a few limitations outlined below.
The Script Engine treats all lines from the start of the script (inclusive) to the first invalid line (exclusive) as header.
The header is not removed from the file before compiling.

## Valid Header Line

A Header line is considered valid if it matches one of the conditions below:

- Single line comment (`//`) that is not a header directive. There is no support for `/**/` yet
- Empty Line
- Line with only Whitespace
- Line Starting with A header directive (`//#include`, `//#rev`, `//#mode`, `//#version`)
- Line containing a `using` Statement

## Header Directives

Header Directives are `//#include`, `//#ref`, `//#mode` and `//#version`.
The fact that they look like comments is on purpose because the C# Compiler can't handle custom directives.
A Directive is treated as comment and ignored if it starts with at lest 3 slashes.

### include

Formats:

    //#include "script.cs"
    //#include "script.dll"
    //#include <script.cs>
    //#include <script.dll>

This is similar to a C style include with these differences:

- Can be used for compiled scripts, there is no different command for linking binaries
- A specific file is only ever included once
- It can only be used in the script header

If the included file is a script, it will be added to the list of files to compile,
if the file is a binary, it will be added to the list of references.

Using quotes will resolve the path relative to the currently processed script,
using brackets will resolve the path relative to `scs.Tools.ReferencePath`,
as of now this is a subfolder of the engine called "Lib".

`Engine.dll` is included by default and gives your Script the Ability to use the Compiler.

**There are no libraries we ship by default. If you know a good library,
feel free to open an Issue for it**

### ref

This is similar to `include` but is intended to be used to reference .NET Assembiles

Formats:

    //#ref "System.Windows.Forms.dll"

Quotes are required and a path should not be supplied.
Internally it works identical to `#include` with a binary file,
but it doesn't treats the path as being relative to the script file.

`System.dll` and `mscorlib.dll` are referenced by default.

**If you feel that other Libraries should be referenced by default, open an issue**

### mode

This specifies the script mode.
This should usually be the first line in a script but can be placed anywhere in the Header.
Only the first instance of this header is used.

Valid modes are `single`, `simple` and `complex`.
If it is not specified, it defaults to `single`.
A script referenced by `include` must specify `complex`

### version

The version header specifies the minimum Version of the engine this script needs.
It's used for compatibility between engines and scripts.

**Note:** It's reserved for future use and will abort execution if encountered.

# TODO

- [ ] SCS is theoretically only a temporary name. `csc` (C# Compiler) is already the name of the real compiler and
`css` (C# Script) is already a completely different format.
- [ ] Implement `//#version`
- [ ] Build a Library
- [ ] More Documentation than just the readme file.

# Engine Settings Overview

- **.NET Version of Engine and Scripts**: 4.5
- **Default References**: `mscorlib.dll`, `System.dll`
- **Default Script Mode**: single
