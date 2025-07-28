using System.Net;
using CleanResult;
using Mapster;
using Marten;
using Todos.Core;

namespace Todos.Application.Commands.Todos;

public record DeleteTodoCommand(Guid Id);

public class DeleteTodoCommandHandler
{
    public static async Task<Result<Todo>> LoadAsync(DeleteTodoCommand command, IQuerySession session)
    {
        var todo = await session.LoadAsync<Todo>(command.Id);
        if (todo is null)
            return Result.Error("Todo not found", HttpStatusCode.NotFound);
        return Result.Ok(todo);
    }

    public static async Task<Result> Handle(DeleteTodoCommand command, Todo todo, IDocumentSession session)
    {
        session.Delete(todo);
        await session.SaveChangesAsync();

        var asdf = "asdf";
        var asd = asdf.Adapt<Result>();

        return Result.Ok();
    }
}