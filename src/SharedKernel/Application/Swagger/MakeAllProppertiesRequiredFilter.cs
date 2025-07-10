using CleanResult;
using CommunityToolkit.Diagnostics;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SharedKernel.Application.Swagger;

/// <summary>
/// Schema filter to make all properties required in swagger
/// </summary>
public class MakeAllPropertiesRequiredFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        Guard.IsNotNull(schema, "Swagger schema filter");
        if (context.Type != typeof(Error))
            schema.Required = schema.Properties.Keys.ToHashSet();
    }
}