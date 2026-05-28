using CleanResult;
using ManualWolverineHandlerRegistration.Application.Commands;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace ManualWolverineHandlerRegistration.Api;

public class TestApi
{
    [WolverineGet("/test/get/external")]
    public static async Task<Result<string>> GetTestCommand([FromServices] IMessageBus bus)
    {
        var command = new TestCommand();
        var result = await bus.InvokeAsync<Result<string>>(command);
        return result;
    }
}
