using CleanResult;
using Microsoft.AspNetCore.Mvc;
using SwaggerExamples.Api.Requests;
using Wolverine.Http;

namespace SwaggerExamples.Api;

public class SwaggerDemoEndpoints
{
    [WolverinePost("swagger/demo")]
    [ProducesResponseType<Error>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Error>(StatusCodes.Status400BadRequest)]
    public static Result<int> PostDemoData(SwaggerDemoRequest request)
    {
        return Result.Ok(42);
    }
}