using Microsoft.Build.Logging;

var targets = new List<string>();
var reader = new BinaryLogReplayEventSource();
reader.TargetStarted += (_, target) => targets.Add(target.TargetName);
reader.Replay(args[0]);
foreach (string name in targets.Where(name => name is "Build" or "Compile" or "ComputeFilesToPublish"
    or "GenerateEstuaryLegacyManifest" or "GenerateEstuaryPublishManifest" or "IncludeEstuaryManifest"))
    Console.WriteLine(name);
