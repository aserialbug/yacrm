# CD - Class Diagrams для PersonService

## Описание

UML диаграммы классов для каждого слоя сервиса PersonService согласно чистой архитектуре.

## Структура

| Файл | Слой | Описание |
|------|------|----------|
| [PersonDomain.puml](PersonDomain.puml) | Domain | Доменный слой: Entity, Value Objects |
| [PersonApplication.puml](PersonApplication.puml) | Application | Прикладной слой: Services, DTO, Repository interfaces |
| [PersonInfrastructure.puml](PersonInfrastructure.puml) | Infrastructure | Инфраструктурный слой: Controllers, DbContext, Repository implementations |

## Архитектура слоёв

```plantuml
@startuml Layers Architecture
title PersonService - Clean Architecture Layers

package "Domain Layer" #LightBlue {
    [Entity]
    [Value Objects]
    note "Person, FirstName, MiddleName, LastName, SocialLinkValue" as N1
}

package "Application Layer" #LightGreen {
    [Services]
    [DTOs]
    [Repository Interfaces]
    note "PersonAppService, PersonDto, IPersonRepository" as N2
}

package "Infrastructure Layer" #LightYellow {
    [Controllers]
    [DbContext]
    [Repository Implementations]
    note "PersonController, PersonDbContext, PersonRepository" as N3
}

Infrastructure Layer --> Application Layer : использует
Infrastructure Layer --> Domain Layer : использует
Application Layer --> Domain Layer : зависит от

note right
  **Clean Architecture:**
  - Domain - ядро, не зависит ни от чего
  - Application - зависит только от Domain
  - Infrastructure - зависит от обоих
end note

skinparam packageStyle rectangle
@enduml
```

## Ссылки

- [ADR 002 - Service Architecture](../../ADR/ADR%20002%20Service%20architecture.md)
- [ADR 004 - Модель предметной области](../../ADR/ADR%20004%20Модель%20предметной%20области.md)
- [API](../API/) - OpenAPI спецификация
- [PDM](../PDM/) - Физическая модель данных
- [PR](../PR/) - Требования к продукту

## Соответствие ADR 004

Диаграммы доменного слоя соответствуют требованиям ADR 004:

| Требование | Реализация |
|------------|------------|
| Сущности наследуются от Entity | `Person <|-- Entity` |
| Идентификатор — объект-значение | `EntityId` Value Object |
| Приватный конструктор VO | `-FirstName(string value)` |
| Фабричный метод для создания VO | `+static FirstName Create(string value)` |
| Валидация в фабричном методе | `Create()` содержит валидацию |
| Нет одержимости примитивами | Все скаляры обернуты в VO |
