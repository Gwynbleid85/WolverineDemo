using System.Net;
using CleanResult;
using Mapster;
using Marten;
using Todos.Core;
using Todos.Core.Events.Todos;
using Wolverine;

namespace Todos.Application.Commands.Todos;

public record CreateTodoCommand(
    Guid Id,
    string Title,
    string Description,
    bool IsCompleted = false
);

public class CreateTodoCommandHandler
{
    public async Task<Result> LoadAsync(CreateTodoCommand command, IQuerySession session)
    {
        var todo = await session.Query<Todo>().FirstOrDefaultAsync(t => t.Title == command.Title);
        if (todo is not null)
            return Result.Error(
                "Todo with the same title already exists.",
                HttpStatusCode.BadRequest
            );
        return Result.Ok();
    }

    public async Task<Result<TodoCreated>> Handle(
        CreateTodoCommand command,
        IDocumentSession session,
        IMessageBus bus
    )
    {
        var todo = command.Adapt<Todo>();
        session.Store(todo);

        await session.SaveChangesAsync();
        var todoCreatedEvent = todo.Adapt<TodoCreated>();
        await bus.PublishAsync(todoCreatedEvent);

        return Result.Ok(todo.Adapt<TodoCreated>());
    }
}
