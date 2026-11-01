using Playbook.Samples;

RuntimeIdentity.Assert();
await SocksChecks.RunAsync();
Console.WriteLine("PASS CairnRelay: actual net11.0 consumer checks complete");
