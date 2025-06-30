using Mapster;
using Marten;
using Todos.Core;
using Todos.Core.Events;

namespace Todos.Application.Commands;

public record CreateNewTodoCommand(Guid Id, string Title, string Description, bool IsCompleted = false);

public class CreateNewTodoCommandHandler
{
    public static async Task LoadAsync(CreateNewTodoCommand command) { }

    public static async Task<TodoCreated> Handle(CreateNewTodoCommand command, IDocumentSession session)
    {
        var todo = command.Adapt<Todo>();
        session.Store(todo);
        await session.SaveChangesAsync();
        return todo.Adapt<TodoCreated>();
    }
}