using System.Net.Sockets;
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;
using PersonService.Persistence.Models;

namespace PersonService.Persistence.Services;

public class MigrationsService
{
    private const string FindDatabaseByNameQuery = "select datname from pg_database where datname = @database";
    private const string CreateDatabaseCommand = "create database \"{0}\" owner {1}";
    private const string LockMigrationsTableCommand = "lock only _migrations_log in ACCESS EXCLUSIVE mode";
    private const string CreateMigrationsTableCommand = "create table if not exists _migrations_log (id int primary key , applied_at timestamp, description text)";
    private const string GetAppliedMigrationQuery = "select id from _migrations_log";
    private const string AddAppliedMigrationCommand = "insert into _migrations_log (id, applied_at, description) values (@id, @applied, @description)";
    
    private const string SystemDatabaseName = "postgres";
    private readonly IConfiguration _configuration;
    private readonly ILogger<MigrationsService> _logger;
    private readonly MigrationDefinitionsService _migrationsReader;
    private readonly MigrationsServiceSettings _settings;

    public MigrationsService(IConfiguration configuration,
        ILogger<MigrationsService> logger,
        MigrationDefinitionsService migrationsReader,
        IOptions<MigrationsServiceSettings> settings)
    {
        _configuration = configuration;
        _logger = logger;
        _migrationsReader = migrationsReader;
        _settings = settings.Value;
    }

    public async Task Apply()
    {
        int currentAttempt = 1;
        bool shouldRetry = true;
        while (currentAttempt <= _settings.MigrationRetriesCount && shouldRetry)
        {
            try
            {

                _logger.LogInformation("Apply migrations attempt {Number}", currentAttempt);
                await ApplyMigrations();
                
                // Миграции применились, повторять не нужно
                shouldRetry = false;
            }
            catch (NpgsqlException npgsqlException)
            {
                if (npgsqlException.InnerException is SocketException)
                {
                    // Если мы и в последний раз не применили миграцию, то пробрасываем
                    // исключение дальше
                    if (currentAttempt >= _settings.MigrationRetriesCount)
                        throw;
                    
                    _logger.LogError(npgsqlException, "Error applying migration, will retry in {Time} seconds", _settings.MigrationRetryIntervalSec);
                    await Task.Delay(_settings.MigrationRetryIntervalSec * 1000);
                }
                else
                {
                    throw;
                }
            }

            currentAttempt++;
            // Остальные ошибки не обрабатывем т.к. при них пробовать еще раз нет смысла
        }
    }

    private async Task ApplyMigrations()
    {
        var connectionString = ConnectionString.Parse(
            _configuration.GetConnectionString(nameof(PersonServicePostgresContext)));
        
        await EnsureDatabaseCreated(connectionString);

        var migrations = await _migrationsReader.ReadMigrationsDefinitions();
        if (migrations == null || migrations.Length == 0)
        {
            _logger.LogInformation("No migrations found");
            return;
        }
        
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString.ToString());
        await using var multiHostDataSource = dataSourceBuilder.BuildMultiHost();
        await using var dataSource = multiHostDataSource.WithTargetSession(TargetSessionAttributes.Primary);
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await EnsureMigrationLogExists(dataSource);
        await using (var lockCommand = new NpgsqlCommand(LockMigrationsTableCommand, connection, transaction))
        {
            await lockCommand.ExecuteNonQueryAsync();
        }

        HashSet<int> appliedMigrationIds = new();
        await using (var getAppliedMigrationsQuery =
                     new NpgsqlCommand(GetAppliedMigrationQuery, connection, transaction))
        {
            await using (var reader = await getAppliedMigrationsQuery.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    appliedMigrationIds.Add(reader.GetInt32(0));
                }
            }
        }

        var toApply = migrations
            .Where(m => !appliedMigrationIds.Contains(m.Order))
            .OrderBy(m => m.Order);
        foreach (var migration in toApply)
        {
            _logger.LogInformation(
                "Applying migration {Id}: {Description}", 
                migration.Order, 
                migration.Description);
            
            await using (var migrationCommand = new NpgsqlCommand(migration.Body, connection, transaction))
            {
                await migrationCommand.ExecuteNonQueryAsync();
            }

            await using (var addAppliedMigrationCommand =
                         new NpgsqlCommand(AddAppliedMigrationCommand, connection, transaction))
            {
                addAppliedMigrationCommand.Parameters.AddWithValue("id", NpgsqlDbType.Integer, migration.Order);
                addAppliedMigrationCommand.Parameters.AddWithValue("applied", NpgsqlDbType.Timestamp, DateTime.Now);
                addAppliedMigrationCommand.Parameters.AddWithValue("description", migration.Description);
                await addAppliedMigrationCommand.ExecuteNonQueryAsync();
            }
        }
        
        await transaction.CommitAsync();
    }

    private async Task EnsureMigrationLogExists(NpgsqlDataSource npgsqlDataSource)
    {
        await using var command = npgsqlDataSource.CreateCommand(CreateMigrationsTableCommand);
        await command.ExecuteNonQueryAsync();
    }

    private async Task EnsureDatabaseCreated(ConnectionString connectionString)
    {
        await using var dataSource = NpgsqlDataSource.Create(
            connectionString
                .WithDatabase(SystemDatabaseName)
                .ToString());

        await using (var findDbCommand = dataSource.CreateCommand(FindDatabaseByNameQuery))
        {
            findDbCommand.Parameters.AddWithValue("database", connectionString.Database);
            await using (var reader = await findDbCommand.ExecuteReaderAsync())
            {
                if(await reader.ReadAsync())
                    return;   
            }
        }

        _logger.LogInformation("Creating database {Name}", connectionString.Database);
        var sql = string.Format(CreateDatabaseCommand, connectionString.Database, connectionString.User);
        await using (var createDbCommand = dataSource.CreateCommand(sql))
        {
            try
            {
                await createDbCommand.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error creating database: {Message}, " +
                                   "it might be already created, trying to continue...", e.Message);
            }
        }
    }
    
    public record MigrationsServiceSettings
    {
        public const string SectionName = "MigrationsService";

        public int MigrationRetriesCount { get; init; }
        public int MigrationRetryIntervalSec { get; init; }
    }
}