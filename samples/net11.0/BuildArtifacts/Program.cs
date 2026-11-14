// snippet:archive-helper
using System.Formats.Tar;

if (args is ["archive", string source, string destination])
{
    File.Delete(destination);
    TarFile.CreateFromDirectory(source, destination, includeBaseDirectory: false);
}
else if (args is ["extract", string archive, string directory])
{
    Directory.CreateDirectory(directory);
    TarFile.ExtractToDirectory(archive, directory, overwriteFiles: true);
}
else if (args.Length == 0)
{
    Console.WriteLine("Tidal freight dispatch build");
}
else
{
    throw new ArgumentException("Expected archive <directory> <tar> or extract <tar> <directory>.");
}
// endsnippet:archive-helper
