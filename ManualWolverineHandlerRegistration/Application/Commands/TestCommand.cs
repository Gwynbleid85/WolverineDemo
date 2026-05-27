using CleanResult;

namespace ManualWolverineHandlerRegistration.Application.Commands;

public record TestCommand;

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
