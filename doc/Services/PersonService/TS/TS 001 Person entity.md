# TS 001 - Технические решения для PR 001 "Сущность Person"

## Статус

Разрабатывается

## Ссылки

| Документ | Описание |
|----------|----------|
| [PR 001](../PR/PR%20001%20Person%20entity.md) | Требования к сущности Person |
| [CD/PersonDomain.puml](../CD/PersonDomain.puml) | Диаграмма доменного слоя |
| [CD/PersonApplication.puml](../CD/PersonApplication.puml) | Диаграмма прикладного слоя |
| [CD/PersonInfrastructure.puml](../CD/PersonInfrastructure.puml) | Диаграмма инфраструктурного слоя |
| [PDM/PDM.md](../PDM/PDM.md) | Физическая модель данных |
| [API/person-service-api.yaml](../API/person-service-api.yaml) | OpenAPI спецификация |

## Архитектура проекта

```
PersonService/
├── PersonService.Domain/              # Доменный слой
│   ├── Entities/
│   │   ├── Entity.cs
│   │   └── Person.cs
│   └── ValueObjects/
│       ├── EntityId.cs
│       ├── FirstName.cs
│       ├── MiddleName.cs
│       ├── LastName.cs
│       └── SocialLinkValue.cs
├── PersonService.Application/         # Прикладной слой
│   ├── DTOs/
│   │   ├── PersonDto.cs
│   │   ├── CreatePersonDto.cs
│   │   └── UpdatePersonDto.cs
│   ├── Interfaces/
│   │   └── IPersonRepository.cs
│   └── Services/
│       └── PersonAppService.cs
└── PersonService/                     # Инфраструктурный слой
    ├── Controllers/
    │   └── PersonController.cs
    ├── Persistence/
    │   ├── PersonDbContext.cs
    │   └── Repositories/
    │       └── PersonRepository.cs
    └── Program.cs
```

## Задачи реализации

### 1. Доменный слой (PersonService.Domain)

#### 1.1. Создать проект PersonService.Domain

```bash
dotnet new classlib -n PersonService.Domain -o PersonService.Domain
```

#### 1.2. Создать объект-значение EntityId

**Файл:** `PersonService.Domain/ValueObjects/EntityId.cs`

```csharp
namespace PersonService.Domain.ValueObjects;

public class EntityId
{
    private Guid Value { get; }

    private EntityId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Id не может быть пустым", nameof(value));
        
        Value = value;
    }

    public static EntityId Create(Guid value)
    {
        return new EntityId(value);
    }

    public static EntityId New()
    {
        return new EntityId(Guid.NewGuid());
    }

    public override bool Equals(object? obj)
    {
        if (obj is not EntityId other)
            return false;
        
        return Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator Guid(EntityId entityId)
    {
        return entityId.Value;
    }

    public static implicit operator EntityId(Guid guid)
    {
        return new EntityId(guid);
    }
}
```

#### 1.3. Создать абстрактный класс Entity

**Файл:** `PersonService.Domain/Entities/Entity.cs`

```csharp
using PersonService.Domain.ValueObjects;

namespace PersonService.Domain.Entities;

public abstract class Entity
{
    public EntityId Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime UpdatedAt { get; protected set; }

    protected Entity()
    {
        Id = EntityId.New();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    protected Entity(EntityId id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
            return false;
        
        if (ReferenceEquals(this, other))
            return true;
        
        return Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
```

#### 1.4. Создать объекты-значения для имени

**Файл:** `PersonService.Domain/ValueObjects/FirstName.cs`

```csharp
namespace PersonService.Domain.ValueObjects;

public class FirstName
{
    private const int MinLength = 1;
    private const int MaxLength = 50;
    
    private string Value { get; }

    private FirstName(string value)
    {
        Value = value;
    }

    public static FirstName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Имя не может быть пустым", nameof(value));
        
        if (value.Length > MaxLength)
            throw new ArgumentException($"Имя не может превышать {MaxLength} символов", nameof(value));
        
        return new FirstName(value.Trim());
    }

    public override bool Equals(object? obj)
    {
        if (obj is not FirstName other)
            return false;
        
        return Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }
}
```

**Файл:** `PersonService.Domain/ValueObjects/MiddleName.cs`

```csharp
namespace PersonService.Domain.ValueObjects;

public class MiddleName
{
    private const int MaxLength = 50;
    
    private string? Value { get; }

    private MiddleName(string? value)
    {
        Value = value;
    }

    public static MiddleName Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new MiddleName(null);
        
        if (value.Length > MaxLength)
            throw new ArgumentException($"Отчество не может превышать {MaxLength} символов", nameof(value));
        
        return new MiddleName(value.Trim());
    }

    public static MiddleName Empty()
    {
        return new MiddleName(null);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not MiddleName other)
            return false;
        
        return Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value?.GetHashCode() ?? 0;
    }

    public override string? ToString()
    {
        return Value;
    }
}
```

**Файл:** `PersonService.Domain/ValueObjects/LastName.cs`

```csharp
namespace PersonService.Domain.ValueObjects;

public class LastName
{
    private const int MinLength = 1;
    private const int MaxLength = 50;
    
    private string Value { get; }

    private LastName(string value)
    {
        Value = value;
    }

    public static LastName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Фамилия не может быть пустой", nameof(value));
        
        if (value.Length > MaxLength)
            throw new ArgumentException($"Фамилия не может превышать {MaxLength} символов", nameof(value));
        
        return new LastName(value.Trim());
    }

    public override bool Equals(object? obj)
    {
        if (obj is not LastName other)
            return false;
        
        return Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }
}
```

#### 1.5. Создать объект-значение SocialLinkValue

**Файл:** `PersonService.Domain/ValueObjects/SocialLinkValue.cs`

```csharp
namespace PersonService.Domain.ValueObjects;

public class SocialLinkValue
{
    private const int MaxLength = 255;
    
    private string? Value { get; }

    private SocialLinkValue(string? value)
    {
        Value = value;
    }

    public static SocialLinkValue Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new SocialLinkValue(null);
        
        if (value.Length > MaxLength)
            throw new ArgumentException($"Ссылка не может превышать {MaxLength} символов", nameof(value));
        
        if (!Uri.TryCreate(value, UriKind.Absolute, out _))
            throw new ArgumentException("Некорректный формат URL", nameof(value));
        
        return new SocialLinkValue(value.Trim());
    }

    public static SocialLinkValue Empty()
    {
        return new SocialLinkValue(null);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not SocialLinkValue other)
            return false;
        
        return Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value?.GetHashCode() ?? 0;
    }

    public override string? ToString()
    {
        return Value;
    }
}
```

#### 1.6. Создать сущность Person

**Файл:** `PersonService.Domain/Entities/Person.cs`

```csharp
using PersonService.Domain.ValueObjects;

namespace PersonService.Domain.Entities;

public class Person : Entity
{
    public FirstName FirstName { get; private set; }
    public MiddleName MiddleName { get; private set; }
    public LastName LastName { get; private set; }
    public SocialLinkValue SocialLink { get; private set; }

    private Person() { }

    public static Person Create(
        FirstName firstName,
        MiddleName middleName,
        LastName lastName,
        SocialLinkValue? socialLink = null)
    {
        return new Person
        {
            Id = EntityId.New(),
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            SocialLink = socialLink ?? SocialLinkValue.Empty(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void ChangeName(FirstName firstName, MiddleName middleName, LastName lastName)
    {
        FirstName = firstName;
        MiddleName = middleName;
        LastName = lastName;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSocialLink(SocialLinkValue? socialLink)
    {
        SocialLink = socialLink ?? SocialLinkValue.Empty();
        UpdatedAt = DateTime.UtcNow;
    }
}
```

### 2. Прикладной слой (PersonService.Application)

#### 2.1. Создать проект PersonService.Application

```bash
dotnet new classlib -n PersonService.Application -o PersonService.Application
```

#### 2.2. Добавить зависимость от доменного слоя

```xml
<!-- PersonService.Application.csproj -->
<ItemGroup>
  <ProjectReference Include="..\PersonService.Domain\PersonService.Domain.csproj" />
</ItemGroup>
```

#### 2.3. Создать DTO

**Файл:** `PersonService.Application/DTOs/PersonDto.cs`

```csharp
namespace PersonService.Application.DTOs;

public class PersonDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = null!;
    public string? SocialLink { get; set; }
}
```

**Файл:** `PersonService.Application/DTOs/CreatePersonDto.cs`

```csharp
namespace PersonService.Application.DTOs;

public class CreatePersonDto
{
    public string FirstName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = null!;
    public string? SocialLink { get; set; }
}
```

**Файл:** `PersonService.Application/DTOs/UpdatePersonDto.cs`

```csharp
namespace PersonService.Application.DTOs;

public class UpdatePersonDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = null!;
    public string? SocialLink { get; set; }
}
```

#### 2.4. Создать интерфейс репозитория

**Файл:** `PersonService.Application/Interfaces/IPersonRepository.cs`

```csharp
using PersonService.Domain.Entities;

namespace PersonService.Application.Interfaces;

public interface IPersonRepository
{
    Task<Person?> GetByIdAsync(Guid id);
    Task<IEnumerable<Person>> GetAllAsync();
    Task<Person> CreateAsync(Person person);
    Task<Person> UpdateAsync(Person person);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
```

#### 2.5. Создать сервис приложения

**Файл:** `PersonService.Application/Services/PersonAppService.cs`

```csharp
using PersonService.Application.DTOs;
using PersonService.Application.Interfaces;
using PersonService.Domain.Entities;
using PersonService.Domain.ValueObjects;

namespace PersonService.Application.Services;

public class PersonAppService
{
    private readonly IPersonRepository _repository;

    public PersonAppService(IPersonRepository repository)
    {
        _repository = repository;
    }

    public async Task<PersonDto?> GetPersonAsync(Guid id)
    {
        var person = await _repository.GetByIdAsync(id);
        return person != null ? MapToDto(person) : null;
    }

    public async Task<IEnumerable<PersonDto>> GetAllPersonsAsync()
    {
        var persons = await _repository.GetAllAsync();
        return persons.Select(MapToDto);
    }

    public async Task<PersonDto> CreatePersonAsync(CreatePersonDto dto)
    {
        var person = Person.Create(
            FirstName.Create(dto.FirstName),
            MiddleName.Create(dto.MiddleName),
            LastName.Create(dto.LastName),
            SocialLinkValue.Create(dto.SocialLink)
        );

        var created = await _repository.CreateAsync(person);
        return MapToDto(created);
    }

    public async Task<PersonDto?> UpdatePersonAsync(Guid id, UpdatePersonDto dto)
    {
        var person = await _repository.GetByIdAsync(id);
        if (person == null)
            return null;

        person.ChangeName(
            FirstName.Create(dto.FirstName),
            MiddleName.Create(dto.MiddleName),
            LastName.Create(dto.LastName)
        );

        if (dto.SocialLink != null)
            person.SetSocialLink(SocialLinkValue.Create(dto.SocialLink));

        var updated = await _repository.UpdateAsync(person);
        return MapToDto(updated);
    }

    public async Task<bool> DeletePersonAsync(Guid id)
    {
        return await _repository.DeleteAsync(id);
    }

    private static PersonDto MapToDto(Person person)
    {
        return new PersonDto
        {
            Id = person.Id,
            FirstName = person.FirstName.ToString()!,
            MiddleName = person.MiddleName.ToString(),
            LastName = person.LastName.ToString()!,
            SocialLink = person.SocialLink.ToString()
        };
    }
}
```

### 3. Инфраструктурный слой (PersonService)

#### 3.1. Добавить зависимости

```xml
<!-- PersonService.csproj -->
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.0" />
  <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.2" />
</ItemGroup>

<ItemGroup>
  <ProjectReference Include="..\PersonService.Application\PersonService.Application.csproj" />
  <ProjectReference Include="..\PersonService.Domain\PersonService.Domain.csproj" />
</ItemGroup>
```

#### 3.2. Создать DbContext

**Файл:** `PersonService/Persistence/PersonDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using PersonService.Domain.Entities;

namespace PersonService.Persistence;

public class PersonDbContext : DbContext
{
    public PersonDbContext(DbContextOptions<PersonDbContext> options)
        : base(options)
    {
    }

    public DbSet<Person> Persons => Set<Person>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Person>(entity =>
        {
            entity.ToTable("persons");
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("uuid")
                .HasConversion(
                    v => v.ToString(),
                    v => Guid.Parse(v))
                .ValueGeneratedOnAdd();
            
            entity.OwnsOne(e => e.FirstName, vo =>
            {
                vo.Property(v => v.Value)
                    .HasColumnName("first_name")
                    .HasMaxLength(50)
                    .IsRequired();
            });

            entity.OwnsOne(e => e.MiddleName, vo =>
            {
                vo.Property(v => v.Value)
                    .HasColumnName("middle_name")
                    .HasMaxLength(50)
                    .IsRequired(false);
            });

            entity.OwnsOne(e => e.LastName, vo =>
            {
                vo.Property(v => v.Value)
                    .HasColumnName("last_name")
                    .HasMaxLength(50)
                    .IsRequired();
            });

            entity.OwnsOne(e => e.SocialLink, vo =>
            {
                vo.Property(v => v.Value)
                    .HasColumnName("social_link")
                    .HasMaxLength(255)
                    .IsRequired(false);
            });

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();
        });
    }
}
```

#### 3.3. Создать репозиторий

**Файл:** `PersonService/Persistence/Repositories/PersonRepository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using PersonService.Application.Interfaces;
using PersonService.Domain.Entities;
using PersonService.Persistence;

namespace PersonService.Persistence.Repositories;

public class PersonRepository : IPersonRepository
{
    private readonly PersonDbContext _context;

    public PersonRepository(PersonDbContext context)
    {
        _context = context;
    }

    public async Task<Person?> GetByIdAsync(Guid id)
    {
        return await _context.Persons.FindAsync(id);
    }

    public async Task<IEnumerable<Person>> GetAllAsync()
    {
        return await _context.Persons.ToListAsync();
    }

    public async Task<Person> CreateAsync(Person person)
    {
        await _context.Persons.AddAsync(person);
        await _context.SaveChangesAsync();
        return person;
    }

    public async Task<Person> UpdateAsync(Person person)
    {
        _context.Persons.Update(person);
        await _context.SaveChangesAsync();
        return person;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var person = await _context.Persons.FindAsync(id);
        if (person == null)
            return false;

        _context.Persons.Remove(person);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Persons.AnyAsync(p => p.Id == id);
    }
}
```

#### 3.4. Обновить контроллер

**Файл:** `PersonService/Controllers/PersonController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using PersonService.Application.DTOs;
using PersonService.Application.Services;

namespace PersonService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonController : ControllerBase
{
    private readonly PersonAppService _personService;

    public PersonController(PersonAppService personService)
    {
        _personService = personService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PersonDto>>> GetAllPersons()
    {
        var persons = await _personService.GetAllPersonsAsync();
        return Ok(persons);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PersonDto>> GetPerson(Guid id)
    {
        var person = await _personService.GetPersonAsync(id);
        if (person == null)
            return NotFound();
        
        return Ok(person);
    }

    [HttpPost]
    public async Task<ActionResult<PersonDto>> CreatePerson([FromBody] CreatePersonDto dto)
    {
        try
        {
            var person = await _personService.CreatePersonAsync(dto);
            return CreatedAtAction(
                nameof(GetPerson),
                new { id = person.Id },
                person);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PersonDto>> UpdatePerson(Guid id, [FromBody] UpdatePersonDto dto)
    {
        try
        {
            var person = await _personService.UpdatePersonAsync(id, dto);
            if (person == null)
                return NotFound();
            
            return Ok(person);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeletePerson(Guid id)
    {
        var deleted = await _personService.DeletePersonAsync(id);
        if (!deleted)
            return NotFound();
        
        return Ok(deleted);
    }
}
```

#### 3.5. Обновить Program.cs

**Файл:** `PersonService/Program.cs`

```csharp
using PersonService.Application.Services;
using PersonService.Application.Interfaces;
using PersonService.Persistence;
using PersonService.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register DbContext
builder.Services.AddDbContext<PersonDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Application services
builder.Services.AddScoped<PersonAppService>();

// Register Repository
builder.Services.AddScoped<IPersonRepository, PersonRepository>();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
```

#### 3.6. Обновить appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=persondb;Username=postgres;Password=postgres"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

## Скрипт миграции БД

```sql
-- Создайте базу данных
CREATE DATABASE persondb;

-- Примените миграции EF Core
dotnet ef database update --project PersonService --startup-project PersonService
```

## Проверка реализации

1. Соберите решение:
   ```bash
   dotnet build
   ```

2. Запустите сервис:
   ```bash
   dotnet run --project PersonService
   ```

3. Проверьте Swagger UI:
   ```
   http://localhost:5000/swagger
   ```

4. Протестируйте API endpoints согласно [API спецификации](../API/person-service-api.yaml)
