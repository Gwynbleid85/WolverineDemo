namespace Todos.Core.Events;

public record TodoDeleted(Guid Id, string Title, string Description, bool IsCompleted);