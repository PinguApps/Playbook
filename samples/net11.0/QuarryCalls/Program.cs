using Playbook.Samples;

RuntimeIdentity.Assert();
bool guarded = false;
try { CallEmitter.RequireDynamicCode(false); } catch (PlatformNotSupportedException) { guarded = true; }
CallChecks.Check(guarded, "synthetic unavailable-dynamic-code branch");
CallChecks.Run();
SignatureChecks.Run();
Console.WriteLine("PASS QuarryCalls: actual net11.0 consumer checks complete");
