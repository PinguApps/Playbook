using System;
using System.IO;
using System.Threading;

namespace CwmSeedBank.Host
{
    public static class SeedIo
    {
        public static int Main(string[] args)
        {
            string root = args[0];
            string batch = args[1];
            string input = Path.Combine(root, batch + ".seed");
            string contents = File.ReadAllText(input);
            File.WriteAllText(Path.Combine(root, batch + ".report"), contents);
            try { File.ReadAllText(Path.Combine(root, batch + ".missing")); }
            catch (FileNotFoundException) { }
            try { File.WriteAllText(root, "cannot write a directory"); }
            catch (UnauthorizedAccessException) { }
            foreach (string path in Directory.EnumerateFiles(root, "*.seed"))
                if (!path.EndsWith(".seed", StringComparison.Ordinal)) return 3;
            File.WriteAllText(Path.Combine(root, batch + ".started"), "active");
            if (args.Length > 2 && args[2] == "wait") Thread.Sleep(60000);
            return args.Length > 2 && args[2] == "fail" ? 7 : 0;
        }
    }
}
