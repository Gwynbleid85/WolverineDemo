using System.Net;
using CleanResult;
using Marten;
using Todos.Core;

namespace Todos.Application.Commands.Todos;

public record DeleteTodoCommand(Guid Id);

public class DeleteTodoCommandHandler
{
    public static async Task<Result> LoadAsync(DeleteTodoCommand command, IQuerySession session)
    {
        var todo = await session.LoadAsync<Todo>(command.Id);
        if (todo is not null)
            return Result.Error("Todo not found", HttpStatusCode.NotFound);
        return Result.Ok();
    }

    public static async Task<Result> Handle(DeleteTodoCommand command, Todo todo, IDocumentSession session)
    {
        session.Delete(todo);
        await session.SaveChangesAsync();

        return Result.Ok();
    }
}