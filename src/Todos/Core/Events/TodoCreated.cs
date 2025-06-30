namespace Todos.Core.Events;

public record TodoCreated(Guid Id, string Title, string Description, bool IsCompleted);