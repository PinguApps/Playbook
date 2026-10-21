using System.Reflection;
using System.Runtime.Loader;

namespace Playbook.Samples;

// snippet:metadata
public static class ParcelMetadata
{
    public static (bool OwnsContext, string? NullableElement, int ConstructorArity) Inspect()
    {
        string[] platform = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);
        string source = typeof(Parcel<>).Assembly.Location;
        using MetadataLoadContext context = new(new PathAssemblyResolver(platform.Append(source)));
        Assembly assembly = context.LoadFromAssemblyPath(source);
        Assembly core = context.LoadFromAssemblyName(typeof(object).Assembly.GetName());
        Type integer = core.GetType("System.Int32", throwOnError: true)!;
        Type nullableDefinition = core.GetType("System.Nullable`1", throwOnError: true)!;
        Type nullable = nullableDefinition.MakeGenericType(integer);
        Type parcel = assembly.GetType("Playbook.Samples.Parcel`1", throwOnError: true)!.MakeGenericType(integer);
        ConstructorInfo constructor = parcel.GetConstructors().Single();

        // .NET 10 keeps the owning context explicitly; it has no reverse lookup API.
        bool ownsContext = context.GetAssemblies().Contains(assembly);
        // Compare definitions from the same metadata universe, not runtime typeof(Nullable<>).
        Type? element = nullable.IsConstructedGenericType && nullable.GetGenericTypeDefinition() == nullableDefinition
            ? nullable.GetGenericArguments()[0] : null;
        // Constructors have no method type parameters; do not call the throwing base implementation.
        int arity = 0;
        return (ownsContext, element?.FullName, arity);
    }
}

public sealed class Parcel<T>(T payload)
{
    public T Payload { get; } = payload;
}
// endsnippet:metadata

// snippet:location
public static class HarbourModuleLocations
{
    // Keep a host-owned logical label separate from the runtime's physical location.
    public static string Report(Assembly assembly) =>
        assembly.GetName().Name == typeof(HarbourModuleLocations).Assembly.GetName().Name
            ? "harbour://modules/parcel" : assembly.Location;
}
// endsnippet:location

// snippet:custom-type
public sealed class NullableAwareHarbourTokenType : HarbourTokenType
{
    // .NET 10 has no GetNullableUnderlyingType virtual to override.
}
// endsnippet:custom-type
