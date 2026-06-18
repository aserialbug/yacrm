using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using PersonService.Application.Interfaces;
using PersonService.Application.Services;
using PersonService.Converters;
using PersonService.Persistence;
using PersonService.Persistence.Repositories;
using PersonService.Persistence.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Register DbContext
builder.Services.AddDbContext<PersonDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PersonServicePostgresContext")));

// Register Application services
builder.Services.AddScoped<PersonAppService>();

// Register Repository
builder.Services.AddScoped<IPersonRepository, PersonRepository>();

// Keep existing services
builder.Services.AddSingleton<PersonServicePostgresContext>();
builder.Services.AddTransient<MigrationDefinitionsService>();
builder.Services.AddTransient<MigrationsService>();
builder.Services.Configure<MigrationsService.MigrationsServiceSettings>(
    builder.Configuration.GetSection(MigrationsService.MigrationsServiceSettings.SectionName));

// Add Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PersonService API", Version = "v1" });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new JsonDateTimeConverter());
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

var app = builder.Build();

// try
// {
//     app.Logger.LogInformation("Applying database migrations...");
//     var migrations = app.Services.GetRequiredService<MigrationsService>();
//     await migrations.Apply();
//     app.Logger.LogInformation("Migrations are applied");
// }
// catch (Exception e)
// {
//     Console.WriteLine(e);
//     throw;
// }

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PersonService API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
