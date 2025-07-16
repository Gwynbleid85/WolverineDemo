using System.Net;
using CleanResult;
using Mapster;
using Marten;
using Todos.Core;
using Todos.Core.Events.Todos;

namespace Todos.Application.Commands.Todos;

public record CreateNewTodoCommand(Guid Id, string Title, string Description, bool IsCompleted = false);

public class CreateNewTodoCommandHandler
{
    public static async Task<Result<TodoCreated>> LoadAsync(CreateNewTodoCommand command, IQuerySession session)
    {
        var todo = await session.Query<Todo>().FirstOrDefaultAsync(t => t.Title == command.Title);
        if (todo is not null)
            return Result.Error("Todo with the same title already exists.", HttpStatusCode.Conflict);
        return Result.Ok(new TodoCreated(command.Id, command.Title, command.Description, command.IsCompleted));
    }

    public static async Task<Result<TodoCreated>> Handle(CreateNewTodoCommand command, IDocumentSession session)
    {
        var todo = command.Adapt<Todo>();
        session.Store(todo);

        await session.SaveChangesAsync();
        return Result.Ok(todo.Adapt<TodoCreated>());
    }
}