namespace SwaggerExamples.Api.Requests;

public class SwaggerDemoRequest
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public int Age { get; set; }
    public required string Email { get; set; }
    public DateTime DateOfBirth { get; set; }
    public required string Address { get; set; }
    public required string PhoneNumber { get; set; }
    public string? Notes { get; set; } // Optional field
    public required List<string> Tags { get; set; } // List of tags
}