using System.Net;
using CleanResult;
using Mapster;
using Marten;
using Todos.Core;
using Todos.Core.Events.Todos;

namespace Todos.Application.Commands.Todos;

public record UpdateTodoCommand(Guid Id, string Title, string Description, bool IsCompleted);

public class UpdateTodoCommandHandler
{
    public static async Task<Result<Todo>> LoadAsync(UpdateTodoCommand command, IQuerySession session)
    {
        var todo = await session.LoadAsync<Todo>(command.Id);
        if (todo is null)
            return Result.Error("Todo not found.", HttpStatusCode.NotFound);
        return Result.Ok(todo);
    }

    public static async Task<Result<TodoUpdated>> Handle(UpdateTodoCommand command, Todo todo, IDocumentSession session)
    {
        command.Adapt(todo);

        session.Update(todo);
        await session.SaveChangesAsync();

        return Result.Ok(todo.Adapt<TodoUpdated>());
    }
}