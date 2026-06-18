# PersonService - Документация

## Document-First подход

Согласно ADR 001, проект следует подходу Document-First, где документация создается до реализации.

## Структура документации

| Раздел | Описание | Путь |
|--------|----------|------|
| **ADR** | Архитектурные решения | [doc/ADR/](../../ADR/) |
| **PR** | Требования к продукту | [PR/](PR/) |
| **CD** | Диаграммы классов | [CD/](CD/) |
| **PDM** | Физическая модель данных | [PDM/](PDM/) |
| **API** | OpenAPI спецификация | [API/](API/) |
| **TS** | Технические решения | [TS/](TS/) |

## Архитектура сервиса

```
PersonService/
├── PersonService/              # Infrastructure слой (Infrastructure)
│   ├── Controllers/
│   ├── PersonDbContext.cs
│   └── PersonRepository.cs
├── PersonService.Application/  # Application слой
│   ├── Services/
│   ├── DTOs/
│   └── IPersonRepository.cs
└── PersonService.Domain/       # Domain слой
    ├── Entities/
    │   └── Person.cs
    └── ValueObjects/
        ├── FirstName.cs
        ├── MiddleName.cs
        ├── LastName.cs
        └── SocialLinkValue.cs
```

## Ссылки на ADR

| Номер | Название | Статус |
|-------|----------|--------|
| [ADR 001](../../ADR/ADR%20001%20DocumentFirst.md) | Document-First подход | Разрабатывается |
| [ADR 002](../../ADR/ADR%20002%20Service%20architecture.md) | Архитектура сервиса | Разрабатывается |
| [ADR 003](../../ADR/ADR%20003%20Технологии%20в%20проекте.md) | Технологии в проекте | Разрабатывается |
| [ADR 004](../../ADR/ADR%20004%20Модель%20предметной%20области.md) | Модель предметной области | Разрабатывается |

## Текущие требования (PR)

| Номер | Описание | Статус |
|-------|----------|--------|
| [PR 001](PR/PR%20001%20Person%20entity.md) | Сущность Person | Разрабатывается |

## Технологический стек

- **.NET 10** — платформа разработки
- **PostgreSQL 18** — база данных
- **Entity Framework Core** — ORM
- **Npgsql** — provider для PostgreSQL
- **Vue.js** — фронтенд
- **Docker Compose** — развёртывание
