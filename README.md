# Funeral Homes · Microservicio Customers (`fh.api.customer`)

Microservicio de **Customers** de la plataforma Funeral Homes, hecho en **.NET 10** con **Clean Architecture**, **DDD** y **CQRS**.
Es el único responsable de los clientes titulares, sus suscripciones a planes y las personas y mascotas cubiertas (beneficiarios).

## Responsabilidades

* **Clientes titulares:** personas naturales (`INDIVIDUAL`) y empresas (`CORPORATE`), con documento, contacto y estado.
* **Suscripciones:** afiliación a un plan y control del cupo máximo de beneficiarios (`max_beneficiaries`).
* **Beneficiarios:** miembros humanos (`HUMAN`) y mascotas (`PET`), con la **edad derivada** calculada desde la fecha de nacimiento.
* **Auditoría y eventos:** cada alta, cambio o retiro de un beneficiario queda registrado como evento de dominio; la publicación a Kafka para que Financials recalcule la cuota está pendiente.

---

## Arquitectura

Una sola Clean Architecture para todo el microservicio. Las capas son **proyectos** y los módulos
(Customers, CustomerPlans, Beneficiaries, Audit, Messaging) son **carpetas** dentro de cada capa.
Los namespaces siguen la convención proyecto + carpeta (por ejemplo `FH.Customers.Application.Beneficiaries.Commands.AddBeneficiary`).

```text
src/
├── Core/
│   ├── FH.Customers.Domain/            # Entidades, value objects, eventos, excepciones, interfaces de repositorio
│   └── FH.Customers.Application/       # Casos de uso (CQRS), mediador, validadores, DTOs, Result
├── Infrastructure/
│   ├── FH.Customers.Persistence/       # EF Core: CustomerDbContext, configuraciones, repositorios, Unit of Work, migraciones
│   └── FH.Customers.Infrastructure/    # Adaptadores externos (gateways) y datos de ejemplo
└── Presentation/
    └── FH.Customers.Api/               # Controllers REST, Program.cs, Swagger
tests/
├── FH.Customers.Tests/                 # Dominio, validadores, mediador y handlers
└── FH.CustomerPlans.Tests/             # Suscripciones
scripts/                                # smoke-test.ps1, reset-db.ps1
docs/                                   # ENDPOINTS.md y documentación por módulo
```

Dependencias (de afuera hacia adentro): `Api → Application, Persistence, Infrastructure`; `Persistence/Infrastructure → Application → Domain`.
`Domain` no depende de nada.

### Patrones aplicados

| Patrón | Dónde |
|---|---|
| **CQRS** | `Application/<Módulo>/Commands` y `Queries`: cada caso de uso tiene Command/Query, Handler y Validator |
| **Mediator propio** | `Application/Mediator`: `IMediator`, `SimpleMediator`, `IRequest`, `IRequestHandler`; resuelve el Handler por reflexión y ejecuta los validadores antes |
| **FluentValidation** | Un `Validator` por caso de uso, ejecutado por el mediador (devuelve 400 sin llegar al Handler) |
| **Value Objects** | `DocumentId`, `ContactInfo`, `Address` (Customer y Member) y `MaxBeneficiaries`, `SubscriptionPeriod` (Subscription) |
| **Repository** | Interfaces en `Domain/<Módulo>/Repositories`, implementación EF Core en `Persistence` |
| **Unit of Work** | `IUnitOfWork` (`CommitAsync`/`RollbackAsync`) en Application; implementación única en Persistence |
| **Eventos de dominio** | Las entidades generan eventos; `DomainEventDispatcher` los entrega a sus manejadores (auditoría) antes de guardar |
| **Result** | Los Handlers devuelven `Result`/`Result<T>` con código HTTP; el controller lo traduce a la respuesta |

---

## Requisitos y ejecución local

Requisitos: **.NET 10 SDK**, **Docker** (para PostgreSQL) y la herramienta `dotnet ef` (`dotnet tool install --global dotnet-ef`).

```powershell
# 1. PostgreSQL
docker run --name fh-pg -e POSTGRES_PASSWORD=secret -p 5432:5432 -d postgres:16

# 2. Crear la base y las tablas (desde la raíz del repositorio)
.\scripts\reset-db.ps1

# 3. Ejecutar la API (abre Swagger en https://localhost:7035/swagger)
dotnet run --project src/Presentation/FH.Customers.Api --launch-profile https

# 4. Pruebas unitarias y prueba de todos los endpoints (con la API corriendo)
dotnet test
.\scripts\smoke-test.ps1
```

La cadena de conexión está en `src/Presentation/FH.Customers.Api/appsettings.json` (`ConnectionStrings:CustomerDb`).
Si PowerShell bloquea los scripts: `Set-ExecutionPolicy -Scope Process Bypass`.

---

## Esquema de Base de Datos (`fh.db.customer`)

El microservicio utiliza una base de datos relacional dedicada:

```sql
-- 1. Tabla de Clientes Titulares / Cuentas
CREATE TABLE customers (
    id UUID PRIMARY KEY,
    customer_type VARCHAR(20) NOT NULL CHECK (customer_type IN ('INDIVIDUAL', 'CORPORATE')),
    name VARCHAR(255) NOT NULL,
    identification_type VARCHAR(20) NOT NULL,
    identification_number VARCHAR(50) NOT NULL,
    email VARCHAR(100),
    phone VARCHAR(30),
    address VARCHAR(255),
    status VARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (identification_type, identification_number)
);

-- 2. Tabla de Suscripciones
CREATE TABLE customer_subscriptions (
    id UUID PRIMARY KEY,
    customer_id UUID NOT NULL REFERENCES customers(id),
    external_plan_id UUID NOT NULL, -- ID del plan en el microservicio Financials
    max_beneficiaries INT NOT NULL DEFAULT 1,
    start_date DATE NOT NULL,
    end_date DATE,
    status VARCHAR(20) NOT NULL DEFAULT 'ACTIVE'
);

-- 3. Tabla Unificada de Sujetos Cubiertos (Personas / Mascotas)
CREATE TABLE members (
    id UUID PRIMARY KEY,
    subject_type VARCHAR(20) NOT NULL CHECK (subject_type IN ('HUMAN', 'PET')),
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100),
    identification_type VARCHAR(20),
    identification_number VARCHAR(50),
    birth_date DATE NOT NULL,
    derived_age INT NOT NULL, -- Calculado automáticamente
    email VARCHAR(100),
    phone VARCHAR(30)
);

-- 4. Tabla de Beneficiarios Vinculados a la Suscripción
CREATE TABLE beneficiaries (
    id UUID PRIMARY KEY,
    subscription_id UUID NOT NULL REFERENCES customer_subscriptions(id),
    member_id UUID NOT NULL REFERENCES members(id),
    beneficiary_type VARCHAR(20) NOT NULL CHECK (beneficiary_type IN ('PRINCIPAL', 'ASSOCIATED')),
    relationship_type VARCHAR(30) NOT NULL, -- TITULAR, SPOUSE, CHILD, PARENT, EMPLOYEE, PET, OTHER
    status VARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
    joined_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    removed_at TIMESTAMP
);

-- 5. Tabla de Auditoría e Historial de Novedades
CREATE TABLE beneficiary_audit_log (
    id UUID PRIMARY KEY,
    subscription_id UUID NOT NULL REFERENCES customer_subscriptions(id),
    member_id UUID NOT NULL REFERENCES members(id),
    action VARCHAR(30) NOT NULL, -- BENEFICIARY_ADDED, BENEFICIARY_REMOVED, BENEFICIARY_UPDATED
    event_published BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

---

## Eventos de Dominio (Apache Kafka)

Este microservicio emite eventos asíncronos cuando ocurren cambios en los beneficiarios:

### Evento: `customer.beneficiary.added`
```json
{
  "eventId": "123e4567-e89b-12d3-a456-426614174000",
  "eventType": "BENEFICIARY_ADDED",
  "timestamp": "2026-10-03T09:59:00Z",
  "data": {
    "subscriptionId": "987e6543-e21b-12d3-a456-426614174111",
    "memberId": "456e7890-e89b-12d3-a456-426614174222",
    "subjectType": "HUMAN",
    "derivedAge": 65,
    "relationshipType": "PARENT",
    "beneficiaryType": "ASSOCIATED"
  }
}
```
* **Consumidores principales**:
  * **Financials (`fh.api.financial`)**: Recalcula el costo de la cuota periódica del plan.
  * **Notifications (`fh.api.notification`)**: Genera la alerta/correo de confirmación al titular.

---

---

## Endpoints

Todos los endpoints, con sus cuerpos de ejemplo y códigos de respuesta, están en [`docs/ENDPOINTS.md`](docs/ENDPOINTS.md).

| Módulo | Ruta base |
|---|---|
| Customers | `/api/v1/customers` |
| Subscriptions | `/api/v1/subscriptions` |
| Beneficiaries | `/api/v1/subscriptions/{subscriptionId}/beneficiaries` |
| Audit | `/api/v1/subscriptions/{id}/audit-log` · `/api/v1/members/{id}/audit-log` |

## Pruebas

* **Unitarias** (xUnit + Moq): entidades y value objects, validadores, mediador, Handlers de Customers, Subscriptions, Beneficiaries y Auditoría.
* **De extremo a extremo:** `scripts/smoke-test.ps1` ejecuta todos los endpoints contra la API real y verifica el código de cada caso.

## Documentación por módulo

Detalle de reglas de negocio y diseño de cada módulo en [`docs/modules`](docs/modules/README.md).
