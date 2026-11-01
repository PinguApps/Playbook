using Playbook.Samples;

RuntimeIdentity.Assert();
bool guarded = false;
try { CallEmitter.RequireDynamicCode(false); } catch (PlatformNotSupportedException) { guarded = true; }
CallChecks.Check(guarded, "synthetic unavailable-dynamic-code branch");
CallChecks.Run();
Console.WriteLine("PASS QuarryCalls: actual net10.0 consumer checks complete");
