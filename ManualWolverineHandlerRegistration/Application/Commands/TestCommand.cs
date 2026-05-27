using CleanResult;

namespace ManualWolverineHandlerRegistration.Application.Commands;

public record TestCommand;

public class TestCommandHandler
{
    public static async Task<Result> LoadAsync(TestCommand command)
    {
        return Result.Ok();
    }

    public static async Task<Result> Handle(TestCommand command)
    {
        return Result.Ok();
    }
}
