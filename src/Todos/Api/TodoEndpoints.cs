using CleanResult;
using Mapster;
using Todos.Api.Models.Requests;
using Todos.Application.Commands.Todos;
using Todos.Application.Queries.Todos;
using Todos.Core;
using Todos.Core.Events.Todos;
using Wolverine;
using Wolverine.Http;

namespace Todos.Api;

public class TodoEndpoints
{
    /// <summary>
    /// Get all todos in the system.
    /// </summary>
    [WolverineGet("/todos")]
    public static async Task<IEnumerable<Todo>> GetTodos(IMessageBus bus)
    {
        var query = new GetAllTodosQuery();
        return await bus.InvokeAsync<IEnumerable<Todo>>(query);
    }

    /// <summary>
    /// Get a todo by its ID.
    /// </summary>
    [WolverineGet("/todos/{id}")]
    public static async Task<Todo> GetTodoById(Guid id, IMessageBus bus)
    {
        var query = new GetTodoByIdQuery(id);
        return await bus.InvokeAsync<Todo>(query);
    }

    /// <summary>
    /// Create a new todo.
    /// </summary>
    [WolverinePost("/todos")]
    public static async Task<Result<TodoCreated>> CreateTodo(CreateTodoRequest request, IMessageBus bus)
    {
        var command = request.Adapt<CreateNewTodoCommand>();
        return await bus.InvokeAsync<Result<TodoCreated>>(command);
    }

    /// <summary>
    /// Update an existing todo.
    /// </summary>
    [WolverinePut("/todos/{id}")]
    public static async Task<IResult> UpdateTodo(Guid id, UpdateTodoRequest request, IMessageBus bus)
    {
        var command = request.Adapt<UpdateTodoCommand>() with { Id = id };
        return await bus.InvokeAsync<Result<TodoUpdated>>(command);
    }

    /// <summary>
    /// Delete a todo by its ID.
    /// </summary>
    [WolverineDelete("/todos/{id}")]
    public static async Task<IResult> DeleteTodo(Guid id, IMessageBus bus)
    {
        var command = new DeleteTodoCommand(id);
        var res = await bus.InvokeAsync<Result>(command);
        return res;
    }
}