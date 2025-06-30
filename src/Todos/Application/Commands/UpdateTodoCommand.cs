using CommunityToolkit.Diagnostics;
using Mapster;
using Marten;
using Todos.Core;
using Todos.Core.Events;

namespace Todos.Application.Commands;

public record UpdateTodoCommand(Guid Id, string Title, string Description, bool IsCompleted);

public class UpdateTodoCommandHandler
{
    public static async Task<Todo> LoadAsync(UpdateTodoCommand command, IQuerySession session)
    {
        var todo = await session.LoadAsync<Todo>(command.Id);
        Guard.IsNotNull(todo, "Todo to update not found");
        return todo;
    }

    public static async Task<TodoUpdated> Handle(UpdateTodoCommand command, Todo todo, IDocumentSession session)
    {
        command.Adapt(todo);

        session.Update(todo);
        await session.SaveChangesAsync();

        return todo.Adapt<TodoUpdated>();
    }
}