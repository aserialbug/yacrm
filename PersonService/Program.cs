using System.Text.Json.Serialization;
using PersonService.Converters;
using PersonService.Persistence;
using PersonService.Persistence.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<PersonServicePostgresContext>();
builder.Services.AddTransient<MigrationDefinitionsService>();
builder.Services.AddTransient<MigrationsService>();
builder.Services.Configure<MigrationsService.MigrationsServiceSettings>(
    builder.Configuration.GetSection(MigrationsService.MigrationsServiceSettings.SectionName));
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new JsonDateTimeConverter());
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

var app = builder.Build();

try
{
    app.Logger.LogInformation("Applying database migrations...");
    var migrations = app.Services.GetRequiredService<MigrationsService>();
    await migrations.Apply();
    app.Logger.LogInformation("Migrations are applied");
}
catch (Exception e)
{
    Console.WriteLine(e);
    throw;
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
