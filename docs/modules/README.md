# Arquitectura del Microservicio Customers (Clean Architecture)

Una sola Clean Architecture para todo el microservicio. Los **módulos** (Customers, CustomerPlans,
Beneficiaries, Audit, Messaging) son **carpetas dentro de cada capa**, no proyectos aparte.
Las dependencias apuntan hacia adentro: `Api → Application → Domain`.

```text
src/
├── Core/
│   ├── FH.Customers.Domain/            # Entidades, enums, value objects, eventos, excepciones, interfaces de repositorio
│   └── FH.Customers.Application/       # Casos de uso: Commands/Queries (CQRS con mediador propio (`IMediator`/`SimpleMediator`, con FluentValidation integrado)), DTOs, handlers, validadores
├── Infrastructure/
│   ├── FH.Customers.Persistence/       # EF Core: DbContexts, configuraciones, migraciones, repositorios, Unit of Work
│   └── FH.Customers.Infrastructure/    # Adaptadores externos: gateways, mock en memoria, (Kafka pendiente)
└── Presentation/
    └── FH.Customers.Api/               # Controllers REST, Program.cs, Swagger, appsettings
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

> Convención de namespaces (igual que Inmoby): **proyecto + carpeta**. Ejemplo: `FH.Customers.Application.Beneficiaries.Commands.AddBeneficiary`
> está en `FH.Customers.Application/Beneficiaries/Commands/AddBeneficiary/`. Los documentos de cada módulo se escribieron antes de la
> reorganización; si mencionan `FH.Shared`, `FH.Modules.*` o `src/2.Modules/...`, el código ahora está en la carpeta del módulo dentro de cada proyecto.

## Migraciones (un solo DbContext: `CustomerDbContext`, en `FH.Customers.Persistence`)

```bash
dotnet ef migrations add <Nombre> --project src/Infrastructure/FH.Customers.Persistence --startup-project src/Presentation/FH.Customers.Api
dotnet ef database update         --project src/Infrastructure/FH.Customers.Persistence --startup-project src/Presentation/FH.Customers.Api
```

### Primera vez tras los value objects
El modelo cambió (`DocumentId`, `ContactInfo`, `Address` dentro de `customers` y `members`; índice único por tipo + número).
Para recrear la base de desarrollo: `.\scripts\reset-db.ps1` (borra la BD, regenera `Migrations/` y crea las tablas).
Todos los endpoints están documentados en [`../ENDPOINTS.md`](../ENDPOINTS.md).
