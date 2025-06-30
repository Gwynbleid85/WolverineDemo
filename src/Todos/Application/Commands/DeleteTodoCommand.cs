using CommunityToolkit.Diagnostics;
using Mapster;
using Marten;
using Todos.Core;
using Todos.Core.Events;

namespace Todos.Application.Commands;

public record DeleteTodoCommand(Guid Id);

public class DeleteTodoCommandHandler
{
    public static async Task<Todo> LoadAsync(DeleteTodoCommand command, IQuerySession session)
    {
        var todo = await session.LoadAsync<Todo>(command.Id);
        Guard.IsNotNull(todo, "Todo not found");
        return todo;
    }

    public static async Task<TodoDeleted> Handle(DeleteTodoCommand command, Todo todo, IDocumentSession session)
    {
        session.Delete(todo);
        await session.SaveChangesAsync();

        return todo.Adapt<TodoDeleted>();
    }
}