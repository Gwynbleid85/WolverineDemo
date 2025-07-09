namespace Todos.Core.Events.Todos;

public record TodoDeleted(Guid Id, string Title, string Description, bool IsCompleted);