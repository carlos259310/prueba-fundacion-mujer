using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ProductCatalog.Api.Endpoints;

public sealed class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext ctx)
    {
        if (!ctx.Type.IsEnum) return;
        schema.Type = "string";
        schema.Format = null;
        schema.Enum = Enum.GetNames(ctx.Type)
            .Select(n => (IOpenApiAny)new OpenApiString(n))
            .ToList();
    }
}
