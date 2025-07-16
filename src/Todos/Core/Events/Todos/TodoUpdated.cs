namespace Todos.Core.Events.Todos;

public record TodoUpdated(Guid Id, string Title, string Description, bool IsCompleted);