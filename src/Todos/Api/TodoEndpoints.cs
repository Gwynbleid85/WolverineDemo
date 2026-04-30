using CleanResult;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Serilog;
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
        Log.Information("Retrieving all todos from the system.");
        var query = new GetAllTodosQuery();
        return await bus.InvokeAsync<IEnumerable<Todo>>(query);
    }

    /// <summary>
    /// Get a todo by its ID.
    /// </summary>
    /// <param name="id">Id of the todo to get</param>
    [ProducesResponseType<Error>(StatusCodes.Status404NotFound)]
    [WolverineGet("/todos/{id}")]
    public static async Task<Todo> GetTodoById([FromRoute] Guid id, IMessageBus bus)
    {
        var query = new GetTodoByIdQuery(id);
        return await bus.InvokeAsync<Todo>(query);
    }

    /// <summary>
    /// Create a new todo.
    /// </summary>
    [ProducesResponseType<Error>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Error>(StatusCodes.Status404NotFound)]
    [WolverinePost("/todos")]
    public static async Task<Result<TodoCreated>> CreateTodo(
        [FromBody] CreateTodoRequest request,
        IMessageBus bus
    )
    {
        var command = new CreateTodoCommand(
            Guid.CreateVersion7(),
            request.Title,
            request.Description,
            request.IsCompleted == IsCompleted.No
        );
        return await bus.InvokeAsync<Result<TodoCreated>>(command);
    }

    /// <summary>
    /// Update an existing todo.
    /// </summary>
    [ProducesResponseType<Error>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Error>(StatusCodes.Status404NotFound)]
    [WolverinePut("/todos/{id}")]
    public static async Task<Result<TodoUpdated>> UpdateTodo(
        Guid id,
        UpdateTodoRequest request,
        IMessageBus bus
    )
    {
        var command = request.Adapt<UpdateTodoCommand>() with { Id = id };
        return await bus.InvokeAsync<Result<TodoUpdated>>(command);
    }

    /// <summary>
    /// Delete a todo by its ID.
    /// </summary>
    [ProducesResponseType<Error>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Error>(StatusCodes.Status404NotFound)]
    [WolverineDelete("/todos/{id}")]
    public static async Task<Result> DeleteTodo(Guid id, IMessageBus bus)
    {
        var command = new DeleteTodoCommand(id);
        var res = await bus.InvokeAsync<Result>(command);
        return res;
    }
}
