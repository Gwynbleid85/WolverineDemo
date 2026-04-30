namespace Todos.Api.Models.Requests;

public record CreateTodoRequest
{
    public string Title { get; set; }
    public string Description { get; set; }
    public IsCompleted? IsCompleted { get; set; }
}

public enum IsCompleted
{
    Yes,
    No,
    Maybe,
}
