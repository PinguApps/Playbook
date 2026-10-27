// snippet:complete
using System.Reflection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

// Read the same declarations for the older generator's operations and schemas.
namespace FenContracts;

public sealed class LegacyDeprecationTransformer : IOpenApiSchemaTransformer, IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.Description.ActionDescriptor.EndpointMetadata.OfType<ObsoleteAttribute>().Any())
        {
            operation.Deprecated = true;
        }
        return Task.CompletedTask;
    }

    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var obsolete = context.JsonPropertyInfo is { } property
            ? property.AttributeProvider?.IsDefined(typeof(ObsoleteAttribute), true) == true
            : context.JsonTypeInfo.Type.IsDefined(typeof(ObsoleteAttribute), true);
        if (obsolete)
        {
            schema.Deprecated = true;
        }
        return Task.CompletedTask;
    }
}
// endsnippet:complete
