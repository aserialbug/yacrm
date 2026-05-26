using Npgsql;

namespace PersonService.Persistence.Models;

public class ConnectionString
{
    private const string HostParameter = "Host";
    private const string DatabaseParameter = "Database";
    private readonly NpgsqlConnectionStringBuilder _builder;

    public string? Database => _builder.Database;
    public string? User => _builder.Username;
    private ConnectionString(NpgsqlConnectionStringBuilder builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    public object? GetValue(string parameterName)
    {
        if (string.IsNullOrWhiteSpace(parameterName))
            throw new ArgumentNullException(nameof(parameterName));

        if (!_builder.TryGetValue(parameterName, out var value))
            throw new KeyNotFoundException($"Parameter {parameterName} was not found in connection string");

        return value;
    }

    public ConnectionString WithDatabase(string database)
    {
        if (string.IsNullOrWhiteSpace(database))
            throw new ArgumentNullException(nameof(database));
        
        var  newConnectionString = new NpgsqlConnectionStringBuilder();
        foreach (var (key, value) in _builder)
        {
            if(key == DatabaseParameter)
                newConnectionString.Add(DatabaseParameter, database);
            else
            {
                if(value != null)
                    newConnectionString.Add(key, value);
            }
        }

        return new ConnectionString(newConnectionString);
    }

    public override string ToString()
        => _builder.ToString();

    public static ConnectionString Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(nameof(value));
        
        var  builder = new NpgsqlConnectionStringBuilder(value);
        if (!builder.ContainsKey(HostParameter))
            throw new ArgumentException($"Connection string must contain {HostParameter} parameter");
        
        return new ConnectionString(builder);
    }
}