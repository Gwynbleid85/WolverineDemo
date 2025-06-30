namespace Todos.Core.Events;

public record TodoUpdated(Guid Id, string Title, string Description, bool IsCompleted);