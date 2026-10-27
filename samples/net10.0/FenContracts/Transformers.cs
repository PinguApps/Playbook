// snippet:complete
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

// snippet:model
namespace FenContracts;

public sealed class FenDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info.Title = "Alder Fen observations";
        document.Components ??= new OpenApiComponents();
        document.Components.Responses ??= new Dictionary<string, IOpenApiResponse>();
        document.Components.Responses["GaugeOffline"] = new OpenApiResponse { Description = "The observation gauge is offline." };
        document.Extensions ??= new Dictionary<string, IOpenApiExtension>();
        document.Extensions["x-observation-area"] = new JsonNodeExtension(new JsonObject { ["name"] = "Alder Fen", ["revision"] = 7 });
        return Task.CompletedTask;
    }
}

public sealed class FenOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        operation.Responses ??= new OpenApiResponses();
        var response = new OpenApiResponseReference("GaugeOffline");
        OpenApiReferenceWithDescription reference = response.Reference;
        reference.Description = "Retry the gauge later.";
        operation.Responses["503"] = response;
        return Task.CompletedTask;
    }
}

public sealed class FenSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.JsonTypeInfo.Type == typeof(Reading) && context.JsonPropertyInfo is null)
        {
            schema.Extensions ??= new Dictionary<string, IOpenApiExtension>();
            schema.Extensions["x-measurement-system"] = new JsonNodeExtension(JsonValue.Create("metric"));
            foreach (IOpenApiSchema property in schema.Properties?.Values ?? [])
            {
                // A nested value can be a reference. Mutate only inline schemas here.
                if (property is OpenApiSchema inline)
                {
                    inline.Description ??= "Published observation field.";
                }
            }
        }
        return Task.CompletedTask;
    }
}
// endsnippet:model
// endsnippet:complete