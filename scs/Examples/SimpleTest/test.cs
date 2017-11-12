//Use single mode. This means the entire Script is a function
//#mode single

//Include an additional script to test the include feature
//#include "test.include.cs"

//Reference a .NET Assembly
//#ref "System.Windows.Forms.dll"

//Add some usings
using System;
using System.Windows.Forms;

//Header ends below because "Console" is not a valid directive

//Testing basics by writing to Console
Console.WriteLine("Generic Console Access: This is a Test Script");
//Testing access to the "args" Variable, because a "single" script has "string[] args" available
Console.WriteLine("Accessing Arguments: Number of Arguments: {0}", args.Length);
//Show all Arguments
foreach(var arg in args){
	Console.WriteLine("Argument: {0}", arg);
}
//Try to call function from an included file
TestClass.Test();

//Try to call a property from a .NET Assembly
Console.WriteLine("Accessing Referenced .NET Assembly: System.Windows.Forms.Application.MessageLoop={0}",Application.MessageLoop);

//Try to use parts from the script Compiler
Console.WriteLine("Accessing Engine Components: Engine Path: {0}", scs.Tools.Engine);


return 0;//Comments at the end are no problem