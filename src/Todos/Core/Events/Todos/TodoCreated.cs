namespace Todos.Core.Events.Todos;

public record TodoCreated(Guid Id, string Title, string Description, bool IsCompleted);