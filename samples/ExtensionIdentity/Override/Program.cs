using Microsoft.Extensions.Logging;
var a=typeof(ILogger).Assembly;
Console.WriteLine("OVERRIDE " + a.Location + " " + System.Diagnostics.FileVersionInfo.GetVersionInfo(a.Location).ProductVersion);
