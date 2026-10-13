# Arquitectura del Microservicio Customers (Clean Architecture)

Una sola Clean Architecture para todo el microservicio. Los **módulos** (Customers, CustomerPlans,
Beneficiaries, Audit) son **carpetas dentro de `UseCases`** y de `Api/DTOs`, no proyectos aparte.
Las dependencias apuntan hacia adentro: `Api → Application → Domain`.

```text
src/
├── FH.Customers.Domain/            # Entidades, value objects, enums, eventos de dominio, excepciones
├── FH.Customers.Application/       # Contratos (puertos), casos de uso CQRS con mediador propio (`IMediator`/`SimpleMediator`, con FluentValidation integrado), validadores
├── FH.Customers.Persistence/       # EF Core: DbContext, configuraciones, migraciones, repositorios, Unit of Work, seeds
├── FH.Customers.Infrastructure/    # Adaptadores externos: gateways y publicador de eventos en memoria (Kafka pendiente)
└── FH.Customers.Api/               # Controllers REST, DTOs de entrada, Program.cs, Swagger, appsettings
tests/
├── FH.Customers.Tests/
└── FH.CustomerPlans.Tests/
```

| Proyecto | Referencia a | Registro DI |
|---|---|---|
| Domain | (nadie) | — |
| Application | Domain | `AddApplication()` |
| Persistence | Application, Domain | `AddPersistence(configuration)` |
| Infrastructure | Application, Domain | `AddInfrastructure(configuration)` |
| Api | Application, Persistence, Infrastructure | `Program.cs` |

## Módulos (documentación)

| Módulo | Documento |
|---|---|
| Customers | [Customers.md](Customers.md) |
| CustomerPlans (suscripciones) | [CustomerPlans.md](CustomerPlans.md) |
| Beneficiaries | [Beneficiaries.md](Beneficiaries.md) |
| Audit | [Audit.md](Audit.md) |
| Messaging | [Messaging.md](Messaging.md) |

> Convención de namespaces (igual que el microservicio de referencia de la clase, Inmoby): **proyecto + carpeta**. Ejemplo:
> `FH.Customers.Application.UseCases.Beneficiaries.Commands.AddBeneficiary` está en
> `src/FH.Customers.Application/UseCases/Beneficiaries/Commands/AddBeneficiary/`.

## Migraciones (un solo DbContext: `CustomerDbContext`, en `FH.Customers.Persistence`)

```bash
dotnet ef migrations add <Nombre> --project src/FH.Customers.Persistence --startup-project src/FH.Customers.Api
dotnet ef database update         --project src/FH.Customers.Persistence --startup-project src/FH.Customers.Api
```

### Primera vez tras los value objects
El modelo cambió (`DocumentId`, `ContactInfo`, `Address` dentro de `customers` y `members`; índice único por tipo + número).
Para recrear la base de desarrollo: `.\scripts\reset-db.ps1` (borra la BD, regenera `Migrations/` y crea las tablas).
Todos los endpoints están documentados en [`../ENDPOINTS.md`](../ENDPOINTS.md).
