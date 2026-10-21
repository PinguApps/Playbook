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

        bool ownsContext = ReferenceEquals(MetadataLoadContext.GetLoadContext(assembly), context);
        Type? element = nullable.GetNullableUnderlyingType();
        int arity = constructor.GetGenericArguments().Length;
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
    public static void Configure()
    {
        AssemblyLoadContext.SetAssemblyLocationOverride((assembly, originalLocation) =>
            assembly.GetName().Name == typeof(HarbourModuleLocations).Assembly.GetName().Name
                ? "harbour://modules/parcel" : originalLocation);
    }

    public static string Report(Assembly assembly) => assembly.Location;
}
// endsnippet:location
