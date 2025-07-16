using Marten;
using Todos.Core;

namespace Todos.Application.Queries.Todos;

/// <summary>
/// Get a todo by its ID.
/// </summary>
public record GetTodoByIdQuery(Guid Id);

public class GetTodoByIdQueryHandler
{
    public static async Task<Todo?> HandleAsync(GetTodoByIdQuery query, IQuerySession session)
    {
        return await session.LoadAsync<Todo>(query.Id);
    }
}