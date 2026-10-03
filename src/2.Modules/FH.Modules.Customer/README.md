# Módulo de Customer (`FH.Modules.Customer`)

Este módulo forma parte del microservicio **Customers** (`fh.api.customer`) y está diseñado bajo los principios de **Domain-Driven Design (DDD)** y **Clean Architecture**. Su propósito central es gobernar el ciclo de vida demográfico, legal y de contacto de los clientes titulares.

---

## 1. Responsabilidades del Módulo

El módulo `FH.Modules.Customer` es el **único dueño y fuente de la verdad** para la administración de las cuentas y titulares de planes funerarios:

1. **Gestión Integral de Titulares (B2C y B2B)**:
   - Registro y mantenimiento de personas naturales (`INDIVIDUAL`).
   - Registro y mantenimiento de empresas/personas jurídicas (`CORPORATE`).
2. **Validación de Identidad y Unicidad**:
   - Garantizar la unicidad del número de identificación por tipo de documento a nivel de sistema.
   - Normalización de datos de identidad (cédula, pasaporte, NIT, RUT, etc.).
3. **Mantenimiento Demográfico y de Contacto**:
   - Actualización controlada de canales de comunicación (teléfono, correo electrónico, dirección física).
   - Inmutabilidad estricta de documentos de identificación tras la creación para cumplimiento legal y auditoría.
4. **Gobierno del Ciclo de Vida del Cliente**:
   - Transiciones de estado permitidas: `Active`, `Inactive`, `Suspended`.
   - Control de habilitación para emitir nuevas suscripciones a planes funerarios.

---

## 2. Historias de Usuario (User Stories)

### HU-CUS-01: Registro de Cliente Titular Persona Natural (B2C)
* **Como** Asesor Comercial o Usuario del Canal Digital,  
* **Quiero** registrar a una persona natural con sus datos demográficos e identificación,  
* **Para** habilitarlo como titular de suscripciones a planes de previsión funeraria.

#### Criterios de Aceptación (Gherkin):
* **Escenario 1: Creación exitosa de cliente persona natural**
  - **Dado** que se proporcionan nombre completo, tipo de documento válido, número de documento no registrado previamente, y datos de contacto,
  - **Cuando** se solicita el registro a través de la API,
  - **Entonces** el sistema debe crear la entidad con `CustomerType = Individual`, estado inicial `Active`, generar un UUID único, registrar la fecha de creación en UTC y responder con HTTP 201 Created.
* **Escenario 2: Intento de registro con identificación duplicada**
  - **Dado** que ya existe un cliente registrado con el mismo tipo y número de identificación,
  - **Cuando** se envía la solicitud de creación,
  - **Entonces** el sistema debe rechazar la operación y retornar HTTP 409 Conflict indicando la duplicidad.
* **Escenario 3: Datos obligatorios faltantes o inválidos**
  - **Dado** que el nombre está vacío o el formato del correo electrónico es inválido,
  - **Cuando** se valida la solicitud,
  - **Entonces** el sistema debe rechazar el comando con HTTP 400 Bad Request y la lista de errores de validación.

#### Tareas Técnicas:
- [ ] Implementar `CreateCustomerCommand` y su validador con FluentValidation.
- [ ] Implementar `Customer.Create(...)` respetando invariantes en la capa de Dominio.
- [ ] Verificar existencia previa en `ICustomerRepository.ExistsByIdentificationAsync(...)`.
- [ ] Exponer endpoint `POST /api/v1/customers`.

---

### HU-CUS-02: Registro de Cliente Corporativo (B2B)
* **Como** Ejecutivo de Cuentas Corporativas,  
* **Quiero** registrar una empresa con su NIT/RUT y razón social,  
* **Para** que la organización pueda adquirir planes colectivos para sus colaboradores.

#### Criterios de Aceptación:
* **Escenario 1: Creación de cliente corporativo exitosa**
  - **Dado** un NIT corporativo, razón social y datos de sede principal válidos,
  - **Cuando** se envía el comando con `CustomerType = Corporate`,
  - **Entonces** se crea la cuenta corporativa con estado `Active` y se genera el registro en la base de datos.
* **Escenario 2: NIT duplicado**
  - **Dado** una empresa ya existente en base de datos con el mismo NIT,
  - **Cuando** se intenta volver a registrar,
  - **Entonces** se responde con HTTP 409 Conflict.

---

### HU-CUS-03: Consulta de Ficha del Cliente
* **Como** Agente de Servicio o Microservicio dependiente,  
* **Quiero** consultar los datos de un cliente mediante su ID único o su número de documento,  
* **Para** validar su estado actual y detalles de contacto antes de operar sobre sus pólizas.

#### Criterios de Aceptación:
* **Escenario 1: Cliente encontrado por ID**
  - **Dado** un UUID de cliente existente en el sistema,
  - **Cuando** se consulta `GET /api/v1/customers/{id}`,
  - **Entonces** el sistema retorna HTTP 200 OK con el DTO detallado del cliente.
* **Escenario 2: Cliente no encontrado**
  - **Dado** un identificador que no existe en la base de datos,
  - **Cuando** se ejecuta la consulta,
  - **Entonces** se debe responder con HTTP 404 Not Found.

#### Tareas Técnicas:
- [ ] Implementar `GetCustomerByIdQuery` y `GetCustomerByIdentificationQuery`.
- [ ] Implementar métodos en el repositorio `ICustomerRepository.GetByIdAsync(...)`.
- [ ] Exponer endpoints `GET /api/v1/customers/{id}` y `GET /api/v1/customers/by-identification`.

---

### HU-CUS-04: Actualización de Datos Demográficos y de Contacto
* **Como** Titular del Plan o Agente de Operaciones,  
* **Quiero** actualizar el teléfono, correo electrónico y dirección de residencia del titular,  
* **Para** asegurar que las notificaciones y cobros lleguen a los canales correctos.

#### Criterios de Aceptación:
* **Escenario 1: Actualización válida de datos de contacto**
  - **Dado** un cliente existente en estado `Active`,
  - **Cuando** se envía el comando `PUT /api/v1/customers/{id}` con nuevos valores de email, teléfono o dirección,
  - **Entonces** el sistema actualiza dichos campos mediante el método de dominio `UpdateDemographics(...)` y retorna HTTP 200 OK o 204 No Content.
* **Escenario 2: Intento de alteración de identificación**
  - **Dado** un intento de modificar el número o tipo de identificación en el payload de actualización demográfica,
  - **Cuando** se procesa la solicitud,
  - **Entonces** la identificación debe permanecer inalterada (inmutable por diseño de dominio).

---

### HU-CUS-05: Modificación del Estado del Cliente (Suspender / Inactivar / Reactivar)
* **Como** Oficial de Cumplimiento o Administrador,  
* **Quiero** suspender o reactivar la cuenta de un cliente,  
* **Para** bloquear transacciones o rehabilitar el servicio según su condición comercial o legal.

#### Criterios de Aceptación:
* **Escenario 1: Suspensión de cuenta**
  - **Dado** un cliente activo,
  - **Cuando** se solicita su suspensión indicando el motivo,
  - **Entonces** el cliente pasa a estado `CustomerStatus.Suspended`.

---

## 3. Explicación Técnica y Arquitectura

### 3.1 Estructura Interna del Módulo (DDD)
El módulo reside en `src/2.Modules/FH.Modules.Customer` y respeta la arquitectura en capas:

```text
FH.Modules.Customer/
├── domain/                      # Reglas de negocio puras e invariantes
│   ├── Entities/                # Customer (derivado de AggregateRoot<Guid>)
│   ├── Enums/                   # CustomerType, CustomerStatus
│   ├── Repositories/            # Contrato ICustomerRepository
│   └── Exceptions/              # CustomerNotFoundException, DuplicateCustomerException
│
├── application/                 # Orquestación de casos de uso (CQRS)
│   ├── Commands/
│   │   ├── CreateCustomer/      # Command, CommandHandler, Validator
│   │   ├── UpdateDemographics/  # Command, CommandHandler, Validator
│   │   └── ChangeStatus/        # Command, CommandHandler
│   ├── Queries/
│   │   ├── GetCustomerById/     # Query, QueryHandler, DTO
│   │   └── GetCustomerByIdentification/
│   └── Mappings/                # Mapeo Entidad <-> DTO
│
├── infrastructure/              # Adaptadores de persistencia y externos
│   ├── Repositories/            # Implementación CustomerRepository con EF Core
│   └── Persistence/             # Mapeos IEntityTypeConfiguration<Customer>
│
└── README.md                    # Documentación técnica del módulo
```

### 3.2 Modelo de Datos (`fh.db.customer`)
El módulo mapea directamente contra la tabla `customers`:

```sql
CREATE TABLE customers (
    id UUID PRIMARY KEY,
    customer_type VARCHAR(20) NOT NULL CHECK (customer_type IN ('INDIVIDUAL', 'CORPORATE')),
    name VARCHAR(255) NOT NULL,
    identification_type VARCHAR(20) NOT NULL,
    identification_number VARCHAR(50) NOT NULL UNIQUE,
    email VARCHAR(100),
    phone VARCHAR(30),
    address VARCHAR(255),
    status VARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE UNIQUE INDEX uq_customers_identification 
ON customers(identification_type, identification_number);
```

### 3.3 Contratos y Endpoints REST

| Método | Endpoint | Descripción | Respuesta Exitosa | Códigos de Error |
|---|---|---|---|---|
| `POST` | `/api/v1/customers` | Registrar nuevo titular (B2C/B2B) | `201 Created` | `400`, `409` |
| `GET` | `/api/v1/customers/{id}` | Consultar cliente por UUID | `200 OK` | `404` |
| `GET` | `/api/v1/customers/by-identification` | Consultar por tipo y número de documento | `200 OK` | `400`, `404` |
| `PUT` | `/api/v1/customers/{id}` | Actualizar datos demográficos | `200 OK` | `400`, `404` |
| `PATCH` | `/api/v1/customers/{id}/status` | Cambiar estado (Active/Suspended/Inactive) | `204 No Content` | `400`, `404` |

#### Payload de Ejemplo (`POST /api/v1/customers`):
```json
{
  "customerType": "Individual",
  "name": "Carlos Rodríguez",
  "identificationType": "CC",
  "identificationNumber": "1020304050",
  "email": "carlos.rodriguez@example.com",
  "phone": "+573001234567",
  "address": "Calle 45 # 12-34, Bogotá"
}
```

### 3.4 Invariantes y Reglas de Dominio
- **Identificación Inmutable**: Una vez creado el titular, su identificación no se altera directamente. Si hubiese un error tipográfico en el documento, se requiere proceso formal de rectificación administrativa.
- **Normalización**: Los correos electrónicos se almacenan en minúsculas (`ToLowerInvariant()`) y los números de documento sin espacios ni caracteres especiales innecesarios.
- **Integración con Shared**: Reutiliza las entidades base de `FH.Shared.Domain.Entities.Customer` y `AggregateRoot<TId>` para garantizar consistencia con `CustomerDbContext`.
