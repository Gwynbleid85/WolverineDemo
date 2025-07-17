using CleanResult;
using SwaggerExamples.Api.Requests;
using Wolverine.Http;

namespace SwaggerExamples.Api;

public class SwaggerDemoEndpoints
{
    [WolverinePost("swagger/demo")]
    public static Result<int> PostDemoData(SwaggerDemoRequest request)
    {
        return Result.Ok(42);
    }
}