using Marten;
using Todos.Core;

namespace Todos.Application.Queries;

/// <summary>
/// Get all todos in the system.
/// </summary>
public record GetAllTodosQuery;

public class GetAllTodosQueryHandler
{
    public static async Task<IEnumerable<Todo>> Handle(GetAllTodosQuery query, IQuerySession session)
    {
        return await session.Query<Todo>().ToListAsync();
    }
}