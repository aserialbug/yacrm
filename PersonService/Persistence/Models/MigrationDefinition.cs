namespace PersonService.Persistence.Models;

public record MigrationDefinition(int Order, string Description, string Body);