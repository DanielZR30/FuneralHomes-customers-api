# Módulo de Customer (`FH.Modules.Customer`)

Módulo del microservicio **Customers** (`fh.api.customer`).Gobierna el ciclo de vida demográfico, legal y de contacto de los clientes titulares de planes funerarios, bajo **DDD**, **Clean Architecture** y **CQRS** con MediatR.

---

## 1. Responsabilidades

1. **Gestión integral de titulares** (B2C y B2B): registro y mantenimiento de personas naturales (`Individual`) y personas jurídicas (`Corporate`).
2. **Validación de identidad y unicidad**: unicidad de la pareja `(tipo de documento, número)` y normalización de la identificación.
3. **Mantenimiento demográfico y de contacto**: actualización de nombre, correo, teléfono y dirección.
4. **Gobierno del ciclo de vida**: control de las transiciones `Active`, `Suspended` e `Inactive`.

La entidad de dominio y su configuración de persistencia viven en `FH.Shared` (`FH.Shared.Domain.Entities.Customer`), porque `CustomerDbContext` es compartido con los módulos de Suscripciones y Planes. El módulo aporta el **caso de uso**; el núcleo conserva el **modelo**.

---

## 2. Reglas de negocio implementadas

| Regla | Descripción | Respuesta HTTP |
|---|---|---|
| RN-01 | El nombre del titular es obligatorio y no puede ser blanco | `400` |
| RN-02 | Tipo y número de identificación son obligatorios | `400` |
| RN-03 | La pareja `(tipo, número)` es única en todo el sistema | `409` |
| RN-04 | En la actualización el nombre no puede quedar blanco | `400` |
| RN-05 | La identificación es **inmutable** tras la creación | — (no se expone en el payload de actualización) |
| RN-06 | Solicitar el estado actual es idempotente: `204` sin escribir en base de datos | `204` |
| RN-07 | `Inactive` es terminal: no admite reapertura ni suspensión | `409` |
| RN-08 | La consulta por identificación exige ambos parámetros | `400` |

Normalización aplicada en el límite de datos (`CustomerRepository`): el tipo de documento se recorta y se pasa a mayúsculas, el número se recorta, y el correo electrónico se almacena en minúsculas.

---

## 3. Estructura real del módulo

```text
FH.Modules.Customer/
├── domain/
│   └── Repositories/
│       └── ICustomerRepository.cs      # Contrato especializado (varias operaciones de lectura)
├── application/
│   ├── DTOs/
│   │   └── CustomerDtos.cs             # Requests y Responses
│   ├── Commands/
│   │   ├── CreateCustomer/             # Command + Handler  -> 201
│   │   ├── UpdateCustomerDemographics/ # Command + Handler  -> 200
│   │   └── ChangeCustomerStatus/       # Command + Handler  -> 204
│   └── Queries/
│       ├── GetCustomerById/            # Query + Handler    -> 200
│       ├── GetCustomerByIdentification/# Query + Handler    -> 200
│       └── GetCustomers/               # Query + Handler    -> 200
├── infrastructure/
│   ├── Repositories/
│   │   └── CustomerRepository.cs       # Adaptador EF Core sobre CustomerDbContext
│   └── Endpoints/
│       └── CustomerEndpoints.cs        # Capa HTTP fina, sin lógica de negocio
└── Extensions/
    └── CustomerModuleExtensions.cs     # Registro de DI
```

**Decisiones de diseño relevantes**

- **Sin FluentValidation.** La validación se hace con `Result<T>` (`FH.Shared`) en los handlers, siguiendo el patrón vigente de `FH.Modules.Beneficiaries`. Incorporar FluentValidation aquí sería una decisión de arquitectura que afecta a toda la solución y debe tomarla el equipo, no este módulo.
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

Ejemplos listos para ejecutar en [`FH.Api.Customer.http`](../../1.Api/FH.Api.Customer.http), que cubre los casos felices y **todos** los errores de dominio. Swagger disponible en `/swagger` con la especificación OpenAPI 3.

---

## 5. Pruebas

```bash
dotnet test FuneralHomes.Customers.slnx
```

**36 pruebas, todas en verde.** Suite en `tests/FH.Modules.Customer.Tests`:

- `Domain/CustomerEntityTests.cs`: normalización, estado inicial y transiciones de la entidad.
- `Application/CreateCustomerCommandHandlerTests.cs`: creación, duplicado (RN-03), campos obligatorios y traducción de `DbUpdateException`.
- `Application/UpdateAndChangeStatusCommandHandlerTests.cs`: actualización e inmutabilidad (RN-05), idempotencia (RN-06) y terminalidad (RN-07).
- `Application/QueriesHandlerTests.cs`: consulta por id y por identificación, y listado con filtro.

Además de las pruebas unitarias, los seis endpoints se verificaron end-to-end contra PostgreSQL 16 con la migración `20261003181200_InitialCreate` aplicada, confirmando los códigos de la tabla anterior y las ocho reglas de negocio.

---

## 6. Pendiente de decisión del equipo

> **Discrepancia entre RN-03 y el índice en base de datos.**
> `CustomerConfiguration` declara `IX_customers_identification_number` como `UNIQUE` **solo sobre `identification_number`**, mientras que RN-03 define la unicidad sobre la pareja `(identification_type, identification_number)`.
>
> Consecuencia verificada: un cliente `Corporate` con `NIT = 555000111` es rechazado con `409` si ya existe un `Individual` con `CC = 555000111`. La base de datos es **más estricta** que la regla de negocio.
>
> `CreateCustomerCommandHandler` mitiga el efecto: valida la pareja antes de insertar y traduce un `DbUpdateException` del índice a `409`, de modo que nunca se filtra un `500`. Aun así, **el módulo no puede registrar una combinación legítimamente válida** hasta que se corrija el índice.
>
> Corrección propuesta (una sola migración):
>
> ```sql
> DROP INDEX IX_customers_identification_number;
> CREATE UNIQUE INDEX IX_customers_identification
>     ON customers (identification_type, identification_number);
> ```
>
> No se aplicó porque `FH.Shared` es base compartida y la migración afectaría a los módulos de Suscripciones y Planes.

### Otros puntos a alinear antes de integrar

- **`Program.cs` y `FuneralHomes.Customers.slnx`**: el PR abierto de `customer-plans` (#3) modifica las mismas líneas. Al integrar hay que conservar los ensamblados y proyectos de prueba de ambos módulos.
- **`GET /api/v1/customers` no pagina**: usa el almacén en memoria compartido. Con volumen real conviene añadir paginación y filtro por texto antes de exponerlo a clientes externos.