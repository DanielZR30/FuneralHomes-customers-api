# Módulo de Customer (`Customers`)

Módulo del microservicio **Customers** (`fh.api.customer`).Gobierna el ciclo de vida demográfico, legal y de contacto de los clientes titulares de planes funerarios, bajo **DDD**, **Clean Architecture** y **CQRS** con mediador propio.

---

## 1. Responsabilidades

1. **Gestión integral de titulares** (B2C y B2B): registro y mantenimiento de personas naturales (`Individual`) y personas jurídicas (`Corporate`).
2. **Validación de identidad y unicidad**: unicidad de la pareja `(tipo de documento, número)` y normalización de la identificación.
3. **Mantenimiento demográfico y de contacto**: actualización de nombre, correo, teléfono y dirección.
4. **Gobierno del ciclo de vida**: control de las transiciones `Active`, `Suspended` e `Inactive`.

La entidad `Customer` vive en `Domain/Entities`, su mapeo en `Persistence/Configurations` y los casos de uso en `Application/UseCases/Customers`.

---

## 2. Reglas de negocio implementadas

| Regla | Descripción | Respuesta HTTP |
|---|---|---|
| RN-01 | El nombre del titular es obligatorio y no puede ser blanco | `400` |
| RN-02 | Tipo y número de identificación son obligatorios; el tipo debe ser uno de `CC`, `CE`, `TI`, `RC`, `PA`, `NIT` (enum `IdentificationType`) | `400` |
| RN-03 | La pareja `(tipo, número)` es única en todo el sistema | `409` |
| RN-04 | En la actualización el nombre no puede quedar blanco | `400` |
| RN-05 | La identificación es **inmutable** tras la creación | — (no se expone en el payload de actualización) |
| RN-06 | Solicitar el estado actual es idempotente: `204` sin escribir en base de datos | `204` |
| RN-07 | `Inactive` es terminal: no admite reapertura ni suspensión | `409` |
| RN-08 | La consulta por identificación exige ambos parámetros | `400` |

Normalización: el tipo de documento es el enum `IdentificationType` (se acepta en mayúsculas o minúsculas, por ejemplo `cc`, y se guarda como texto `CC`); `DocumentId` y el repositorio recortan y pasan a mayúsculas el número, y el correo electrónico se almacena en minúsculas.

---

## 3. Estructura real del módulo

```text
Domain/
├── Entities/                    # Customer
└── Common/ValueObjects/         # DocumentId, ContactInfo, Address

Application/
├── Contracts/Repositories/      # ICustomerRepository (contrato especializado de lectura)
└── UseCases/Customers/
    ├── Commands/
    │   ├── CreateCustomer/               # Command + Handler + Validator  -> 201
    │   ├── UpdateCustomerDemographics/   # Command + Handler + Validator  -> 200
    │   └── ChangeCustomerStatus/         # Command + Handler + Validator  -> 204
    ├── Queries/
    │   ├── GetCustomerById/              # Query + Handler  -> 200
    │   ├── GetCustomerByIdentification/  # Query + Handler + Validator  -> 200
    │   └── GetCustomers/                 # Query + Handler  -> 200
    └── Common/                  # CustomerResponse (DTO de salida)

Persistence/
├── Configurations/              # CustomerConfiguration
├── Repositories/                # CustomerRepository (adaptador EF Core sobre CustomerDbContext)
└── Seeds/                       # CustomerSeeder (cliente demo en Development)

Api/
├── Controllers/                 # CustomersController (capa HTTP fina, sin lógica de negocio)
└── DTOs/Customers/              # CreateCustomerRequest, UpdateCustomerRequest, ChangeCustomerStatusRequest
```

**Decisiones de diseño relevantes**

- **FluentValidation por caso de uso.** Cada Command/Query tiene su `Validator`, que el mediador ejecuta antes del Handler; si falla responde `400` sin llegar a la lógica.
- **Normalización en el repositorio, no en los handlers.** Así ningún llamador puede omitirla; los handlers solo orquestan y traducen errores.
- **Sin excepciones propias.** Los handlers devuelven `Result<T>` con el código HTTP ya resuelto; la capa HTTP solo serializa.
- **`Inactive` es terminal en el dominio.** La transición se rechaza en `Customer.Deactivate/Suspend/Activate`.

---

## 4. Endpoints

| Método | Ruta | Descripción | Éxito | Errores |
|---|---|---|---|---|
| `POST` | `/api/v1/customers` | Registrar titular (B2C/B2B) | `201` | `400`, `409` |
| `GET` | `/api/v1/customers` | Listar todos, o filtrar con `?status=` | `200` | `400` |
| `GET` | `/api/v1/customers/{id}` | Consultar por UUID | `200` | `404` |
| `GET` | `/api/v1/customers/by-identification` | Consultar por `?type=&number=` | `200` | `400`, `404` |
| `PUT` | `/api/v1/customers/{id}` | Actualizar datos de contacto y nombre | `200` | `400`, `404` |
| `PATCH` | `/api/v1/customers/{id}/status` | Cambiar estado | `204` | `400`, `404`, `409` |

Ejemplos listos para ejecutar en [`FH.Customers.Api.http`](../../src/FH.Customers.Api/FH.Customers.Api.http), que cubre los casos felices y **todos** los errores de dominio. Swagger disponible en `/swagger` con la especificación OpenAPI 3.

---

## 5. Pruebas

```bash
dotnet test FuneralHomes.Customers.slnx
```

**36 pruebas, todas en verde.** Suite en `tests/FH.Customers.Tests`:

- `Domain/CustomerEntityTests.cs`: normalización, estado inicial y transiciones de la entidad.
- `Application/CreateCustomerCommandHandlerTests.cs`: creación, duplicado (RN-03), campos obligatorios y traducción de `DbUpdateException`.
- `Application/UpdateAndChangeStatusCommandHandlerTests.cs`: actualización e inmutabilidad (RN-05), idempotencia (RN-06) y terminalidad (RN-07).
- `Application/QueriesHandlerTests.cs`: consulta por id y por identificación, y listado con filtro.

Además de las pruebas unitarias, los seis endpoints se verificaron end-to-end contra PostgreSQL 16 con la migración `InitialCreate` aplicada, confirmando los códigos de la tabla anterior y las ocho reglas de negocio.

---

## 6. Estado actual y pendientes

- **Unicidad de la identificación (RN-03):** el índice único de `customers` es compuesto sobre `(identification_type, identification_number)`, de modo que un `NIT` y una `CC` con el mismo número son clientes distintos. El `CreateCustomerCommandHandler` valida la pareja antes de insertar y traduce un `DbUpdateException` del índice a `409`.
- **`GET /api/v1/customers` no pagina:** devuelve todos los clientes (con filtro opcional por estado). Con volumen real conviene añadir paginación y filtro por texto antes de exponerlo a clientes externos.
