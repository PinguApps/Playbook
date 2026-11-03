using System;
using System.IO;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length != 2 || args[0] != "night survey" || !File.Exists("observatory.input"))
            return 9;
        Console.WriteLine("Observatory import: " + args[0]);
        Console.WriteLine("Working directory: " + Environment.CurrentDirectory);
        return int.Parse(args[1]);
    }
}
