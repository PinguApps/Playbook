using Playbook.Samples;

RuntimeIdentity.Assert();
await SocksChecks.RunAsync();
Console.WriteLine("PASS CairnRelay: actual net10.0 consumer checks complete");
