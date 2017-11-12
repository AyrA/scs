//////////////////////////////
// Crerates a form and renders
// an image of your Username
// to its Background
//////////////////////////////

//#ref "System.Windows.Forms.dll"
//#include "ImageTools.cs"
using System;
using System.Windows.Forms;

Application.SetCompatibleTextRenderingDefault(false);
Application.EnableVisualStyles();
var f=new Form();
f.Text = "Example Form from a Script";
f.FormBorderStyle=FormBorderStyle.FixedSingle;
f.BackgroundImage = text2img.ImageTools.RenderString(Environment.UserName);

Application.Run(f);
return 0;