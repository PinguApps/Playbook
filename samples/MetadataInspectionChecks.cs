using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Playbook.Samples;

(bool owns, string? element, int parameterCount, int genericArity) = ParcelMetadata.Inspect();
if (!owns || element != "System.Int32" || parameterCount != 1 || genericArity != 0)
{
    throw new InvalidOperationException("Metadata-only inspection mismatch.");
}

if (Nullable.GetUnderlyingType(new NullableAwareHarbourTokenType()) is not null)
{
    throw new InvalidOperationException("A non-nullable diagnostic token must return null.");
}
#if NET11_0_OR_GREATER
bool defaultRejected = false;
try { Nullable.GetUnderlyingType(new HarbourTokenType()); }
catch (NotSupportedException) { defaultRejected = true; }
if (!defaultRejected) { throw new InvalidOperationException("A direct Type subclass must implement the new virtual."); }
#else
if (Nullable.GetUnderlyingType(new HarbourTokenType()) is not null)
{
    throw new InvalidOperationException("The older non-generic token must return null.");
}
#endif
Console.WriteLine("PASS: direct custom Type default behavior and explicit non-nullable adaptation.");

string sourcePath = typeof(Parcel<>).Assembly.Location;
string coreLocation = typeof(object).Assembly.Location;
#if NET11_0_OR_GREATER
HarbourModuleLocations.Configure();
#endif
AssemblyLoadContext loader = new("harbour-module", isCollectible: true);
try
{
    using FileStream input = File.OpenRead(sourcePath);
    Assembly loaded = loader.LoadFromStream(input);
    if (HarbourModuleLocations.Report(loaded) != "harbour://modules/parcel" || typeof(object).Assembly.Location != coreLocation)
    {
        throw new InvalidOperationException("Host location report mismatch.");
    }
#if NET11_0_OR_GREATER
    if (loaded.Location != "harbour://modules/parcel") { throw new InvalidOperationException("Override missing."); }
    bool rejected = false;
    try { AssemblyLoadContext.SetAssemblyLocationOverride((_, location) => location); }
    catch (InvalidOperationException) { rejected = true; }
    if (!rejected) { throw new InvalidOperationException("Second override should be rejected."); }
    Console.WriteLine("PASS: second location override rejected.");
#else
    if (!string.IsNullOrEmpty(loaded.Location)) { throw new InvalidOperationException("Stream-loaded location should be absent."); }
#endif
}
finally { loader.Unload(); }

Console.WriteLine($"PASS: metadata-only context ownership, nullable Int32, constructor value parameters=1, method generic arity=0 and logical location; {RuntimeInformation.FrameworkDescription}.");
