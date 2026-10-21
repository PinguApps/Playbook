using Playbook.Samples;

ConfigurationRootChecks.Run();
await ReloadChecks.RunAsync();
Console.WriteLine($"PASS configuration binding exclusions; awaited reload completion, coalescing, failure and disposal; runtime={Environment.Version}");
