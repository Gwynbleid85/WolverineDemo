using CleanResult;
using Wolverine.Attributes;

namespace ManualWolverineHandlerRegistration.Application.Commands;

public record TestCommand;

// [WolverineIgnore] Ignore tags must be use since we want to register this handler manually, and we do not want to include it in automatic wolverine discovery
[WolverineIgnore]
public class TestCommandHandler
{
    public static async Task<Result> LoadAsync(TestCommand command)
    {
        ContextLog.Information("Loading command {@TestCommand}", command);
        return Result.Ok();
    }

    public static async Task<Result<string>> Handle(TestCommand command)
    {
        ContextLog.Information("Handling command {@TestCommand}", command);
        return Result.Ok("Hello from external handler");
    }
}
