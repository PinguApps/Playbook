using Playbook.Samples;

RuntimeIdentity.Assert();
SignatureChecks.Run(args);
Console.WriteLine("PASS PermitManifest: actual net11.0 consumer checks complete");
