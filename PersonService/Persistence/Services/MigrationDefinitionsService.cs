using System.Xml.Serialization;
using PersonService.Persistence.Models;

namespace PersonService.Persistence.Services;

public class MigrationDefinitionsService
{
    private const string MigrationsFile = "Migrations.xml";
    
    private readonly ILogger<MigrationDefinitionsService> _logger;

    public MigrationDefinitionsService(ILogger<MigrationDefinitionsService> logger)
    {
        _logger = logger;
    }


    public Task<MigrationDefinition[]?> ReadMigrationsDefinitions()
    {
        MigrationDefinition[]? migrationDefinitions = default;
        XmlSerializer serializer = new XmlSerializer(typeof(MigrationDefinition[]));
        try
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), MigrationsFile);
            using var migrationsFile = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            var result = serializer.Deserialize(migrationsFile);
            if (result != null)
                migrationDefinitions = (MigrationDefinition[])result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error reading {File}: {Error}", MigrationsFile, e.Message);
        }

        return Task.FromResult(migrationDefinitions);
    }
}