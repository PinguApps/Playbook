using Playbook.Samples;

RuntimeIdentity.Assert();
await ResponseChecks.RunAsync();
Console.WriteLine("PASS CairnReports: actual net10.0 consumer checks complete");
