using CleanResult;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SharedKernel.Application.Swagger;

public class CleanResultReturnTypeFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var returnType = context.MethodInfo.ReturnType;
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            returnType = returnType.GetGenericArguments()[0];

        if (returnType == typeof(Result))
        {
            operation.Responses.Remove("200");
            operation.Responses.Add("204", new OpenApiResponse
            {
                Description = "Success",
                Content = null
            });
        }

        if (returnType.IsGenericType &&
            returnType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            operation.Responses.Remove("200");
            operation.Responses.Add("200", new OpenApiResponse
            {
                Description = "Success",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new()
                    {
                        Schema = context.SchemaGenerator.GenerateSchema(
                            returnType.GetGenericArguments()[0], context.SchemaRepository)
                    }
                }
            });
        }
    }
}