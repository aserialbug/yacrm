namespace PersonService.Persistence.Models;

public record MigrationDefinition
{
    public int Order { get; init; }
    public string Description { get; init; }
    public string Body { get; init; }
    public MigrationDefinition(int order, string description, string body)
    {
        Order = order;
        Description = description;
        Body = body;
    }
    
    public MigrationDefinition()
    {
        
    }
}