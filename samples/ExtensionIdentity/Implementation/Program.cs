using Microsoft.Extensions.FileProviders;
using var files=new PhysicalFileProvider(AppContext.BaseDirectory);
if (!files.GetFileInfo("Implementation.dll").Exists) throw new Exception("Implementation failed");
Console.WriteLine("PASS Implementation " + typeof(PhysicalFileProvider).Assembly.Location);
