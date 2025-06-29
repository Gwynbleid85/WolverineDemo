using Wolverine.Http;

namespace SharedKernel.Api;

public static class StatusCheckEndpoint
{
    /// <summary>
    /// Get app health status
    /// </summary>
    [WolverineGet("/healthz")]
    public static IResult GetHealthStatus()
    {
        return Results.Ok();
    }

    /// <summary>
    /// Get app ready status
    /// </summary>
    [WolverineGet("/readyz")]
    public static IResult GetReadyStatus()
    {
        return Results.Ok();
    }
    
    /// <summary>
    /// Redirect to the management portal.
    /// </summary>
    [WolverineGet("/")]
    public static IResult Index()
    {
        return Results.Ok("Ok");
    }
}