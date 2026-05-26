using Npgsql;

namespace PersonService.Persistence;

public class PersonServicePostgresContext : IAsyncDisposable
{
    public NpgsqlDataSource DataSource { get; }
    
    public PersonServicePostgresContext(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        var connectionString = configuration.GetConnectionString(nameof(PersonServicePostgresContext));
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.UseLoggerFactory(loggerFactory);
        DataSource = dataSourceBuilder.Build();
    }
    public async ValueTask DisposeAsync()
    {
        await DataSource.DisposeAsync();
    }
}