namespace Todos.Api.Models.Requests;

public record CreateTodoRequest
{
    public string Title { get; set; }
    public string Description { get; set; }
    public bool IsCompleted { get; set; }
}