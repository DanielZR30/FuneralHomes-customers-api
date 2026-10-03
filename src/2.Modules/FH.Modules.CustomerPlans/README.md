# Módulo de Customer Plans (`FH.Modules.CustomerPlans`)

Este módulo forma parte del microservicio **Customers** (`fh.api.customer`) y está enfocado en la administración de **suscripciones a planes funerarios y control estricto de cupos**.

---

## 1. Responsabilidades del Módulo

El módulo `FH.Modules.CustomerPlans` es el responsable de gobernar el vínculo contractual entre un titular y un plan de previsión funeraria:

1. **Gestión del Ciclo de Vida de Suscripciones**:
   - Creación, activación, suspensión, cancelación y expiración de suscripciones (`customer_subscriptions`).
   - Mantenimiento del historial de planes asociados a cada titular.
2. **Control de Cupos de Cobertura (`max_beneficiaries`)**:
   - Fijar y modificar el límite superior de beneficiarios permitidos por póliza/suscripción.
   - Proteger el cupo para que nunca sea inferior al número de beneficiarios activos vinculados actualmente.
3. **Control de Fechas y Vigencia**:
   - Registro de fecha de inicio (`start_date`) y fecha de fin/vencimiento (`end_date`).
4. **Vínculo con Microservicios Externos (Financials)**:
   - Mantener la referencia desacoplada al plan del catálogo comercial mediante el identificador externo `external_plan_id` (perteneciente a `fh.api.financial`).
5. **Servicio Interno de Validación de Elegibilidad**:
   - Proveer contratos y consultas rápidas para que el módulo de Beneficiarios (`FH.Modules.Beneficiaries`) verifique que la suscripción está activa y dispone de cupos libres antes de incorporar miembros.

---

## 2. Historias de Usuario (User Stories)

### HU-PLN-01: Suscripción de Cliente Titular a un Plan Funerario
* **Como** Asesor de Ventas o Titular en autoservicio,  
* **Quiero** suscribir a un cliente activo a un plan funerario especificando su cupo de beneficiarios,  
* **Para** habilitar la cobertura de previsión exequial familiar o empresarial.

#### Criterios de Aceptación:
* **Escenario 1: Creación exitosa de suscripción**
  - **Dado** un cliente existente con estado `CustomerStatus.Active` y un `external_plan_id` válido,
  - **Cuando** se envía la solicitud con un cupo `max_beneficiaries >= 1` y una fecha de inicio `>= hoy`,
  - **Entonces** el sistema crea la suscripción con estado `SubscriptionStatus.Active`, genera un UUID propio y responde con HTTP 201 Created.
* **Escenario 2: Cliente inexistente o inactivo**
  - **Dado** un `customer_id` que no existe o cuyo titular se encuentra suspendido/inactivo,
  - **Cuando** se intenta crear la suscripción,
  - **Entonces** el sistema rechaza la operación con HTTP 400 Bad Request o 422 Unprocessable Entity indicando que el cliente no es elegible.
* **Escenario 3: Cupo de beneficiarios inválido**
  - **Dado** un valor de `max_beneficiaries` menor a 1,
  - **Cuando** se valida el comando,
  - **Entonces** se rechaza con HTTP 400 Bad Request.

#### Tareas Técnicas:
- [ ] Implementar `SubscribeCustomerCommand` y su validador.
- [ ] Verificar estado del cliente antes de registrar la suscripción.
- [ ] Persistir la entidad `CustomerSubscription` en base de datos.
- [ ] Exponer endpoint `POST /api/v1/subscriptions`.

---

### HU-PLN-02: Consulta de Suscripciones de un Cliente
* **Como** Agente de Servicio al Cliente,  
* **Quiero** listar todas las suscripciones (vigentes e históricas) de un cliente determinado,  
* **Para** conocer las pólizas que tiene contratadas y sus cupos asignados.

#### Criterios de Aceptación:
* **Escenario 1: Cliente con una o más suscripciones**
  - **Dado** un `customerId` con suscripciones registradas,
  - **Cuando** se consulta `GET /api/v1/customers/{customerId}/subscriptions`,
  - **Entonces** se retorna HTTP 200 OK con la lista detallada de suscripciones, fechas y estados.
* **Escenario 2: Cliente sin suscripciones**
  - **Dado** un cliente válido sin suscripciones activas ni previas,
  - **Cuando** se realiza la consulta,
  - **Entonces** el sistema retorna HTTP 200 OK con una lista vacía.

---

### HU-PLN-03: Cancelación Voluntaria de Suscripción
* **Como** Titular del Plan o Administrador de Contratos,  
* **Quiero** solicitar la cancelación de una suscripción activa,  
* **Para** dar por terminada la relación contractual de previsión funeraria.

#### Criterios de Aceptación:
* **Escenario 1: Cancelación exitosa**
  - **Dado** una suscripción en estado `Active`,
  - **Cuando** se solicita su cancelación indicando fecha de retiro,
  - **Entonces** su estado cambia a `Cancelled`, se fija la fecha de finalización (`end_date`) y se retorna HTTP 200 OK.
* **Escenario 2: Cancelación sobre suscripción ya inactiva**
  - **Dado** una suscripción que ya está en estado `Cancelled` o `Expired`,
  - **Cuando** se intenta cancelar nuevamente,
  - **Entonces** se rechaza la solicitud informando que la suscripción no se encuentra activa.

---

### HU-PLN-04: Modificación del Cupo Máximo de Beneficiarios (Upgrade / Downgrade)
* **Como** Titular del Plan,  
* **Quiero** ampliar o reducir el cupo máximo de beneficiarios de mi suscripción,  
* **Para** adecuar la cobertura al crecimiento o ajuste de mi grupo familiar.

#### Criterios de Aceptación:
* **Escenario 1: Ampliación de cupo (Upgrade)**
  - **Dado** una suscripción activa con cupo de 4 beneficiarios,
  - **Cuando** se solicita ampliar el cupo a 6,
  - **Entonces** el sistema actualiza `max_beneficiaries = 6`.
* **Escenario 2: Reducción de cupo por debajo de miembros activos (Downgrade inválido)**
  - **Dado** una suscripción con 4 beneficiarios actualmente activos y asociados,
  - **Cuando** se intenta reducir el cupo a 2 sin haber retirado previamente a los miembros,
  - **Entonces** el sistema rechaza la operación con HTTP 409 Conflict o 422 Unprocessable Entity, exigiendo desafiliar miembros primero.

---

### HU-PLN-05: Validación de Elegibilidad y Disponibilidad de Cupo
* **Como** Módulo interno de Beneficiarios (`FH.Modules.Beneficiaries`),  
* **Quiero** verificar si una suscripción está activa y tiene al menos un cupo disponible,  
* **Para** autorizar o rechazar la admisión de un nuevo sujeto cubierto.

#### Criterios de Aceptación:
* **Escenario 1: Suscripción con cupo disponible**
  - **Dado** una suscripción activa con `max_beneficiaries = 5` y 3 beneficiarios vigentes,
  - **Cuando** el servicio valida la elegibilidad,
  - **Entonces** responde afirmativamente con cupos restantes = 2.

---

## 3. Explicación Técnica y Arquitectura

### 3.1 Estructura Interna del Módulo (DDD)
Ubicación: `src/2.Modules/FH.Modules.CustomerPlans`

```text
FH.Modules.CustomerPlans/
├── domain/                      # Lógica de dominio de suscripciones
│   ├── Entities/                # CustomerSubscription (AggregateRoot<Guid>)
│   ├── Enums/                   # SubscriptionStatus
│   ├── Repositories/            # ICustomerSubscriptionRepository
│   └── Exceptions/              # SubscriptionLimitExceededException, SubscriptionNotActiveException
│
├── application/                 # Casos de uso
│   ├── Commands/
│   │   ├── SubscribeCustomer/   # Command, Handler, Validator
│   │   ├── CancelSubscription/  # Command, Handler
│   │   └── UpdateCapacity/      # Command, Handler
│   ├── Queries/
│   │   ├── GetCustomerSubscriptions/
│   │   └── GetSubscriptionById/
│   └── Services/                # ISubscriptionEligibilityService
│
├── infrastructure/              # Persistencia con EF Core
│   └── Repositories/            # CustomerSubscriptionRepository
│
└── README.md                    # Este archivo
```

### 3.2 Modelo de Base de Datos (`customer_subscriptions`)

```sql
CREATE TABLE customer_subscriptions (
    id UUID PRIMARY KEY,
    customer_id UUID NOT NULL REFERENCES customers(id),
    external_plan_id UUID NOT NULL, -- ID del plan en el microservicio Financials
    max_beneficiaries INT NOT NULL DEFAULT 1,
    start_date DATE NOT NULL,
    end_date DATE,
    status VARCHAR(20) NOT NULL DEFAULT 'ACTIVE'
);

CREATE INDEX idx_subscriptions_customer_id ON customer_subscriptions(customer_id);
CREATE INDEX idx_subscriptions_status ON customer_subscriptions(status);
```

### 3.3 Contratos y Endpoints REST

| Método | Endpoint | Descripción | Respuesta Exitosa |
|---|---|---|---|
| `POST` | `/api/v1/subscriptions` | Crear suscripción para un cliente | `201 Created` |
| `GET` | `/api/v1/customers/{customerId}/subscriptions` | Listar suscripciones del cliente | `200 OK` |
| `GET` | `/api/v1/subscriptions/{id}` | Consultar detalle de suscripción | `200 OK` |
| `PATCH` | `/api/v1/subscriptions/{id}/cancel` | Cancelar suscripción | `200 OK` |
| `PATCH` | `/api/v1/subscriptions/{id}/capacity` | Actualizar cupo de beneficiarios | `200 OK` |

#### Payload de Ejemplo (`POST /api/v1/subscriptions`):
```json
{
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "externalPlanId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "maxBeneficiaries": 5,
  "startDate": "2026-10-05"
}
```

### 3.4 Invariantes y Reglas de Negocio
- **Cupo Mínimo**: `max_beneficiaries` debe ser `>= 1`.
- **Integridad Referencial con Clientes**: No se pueden emitir suscripciones sobre `customer_id` inexistente o cuyo estado sea `Suspended` o `Inactive`.
- **Desacoplamiento con Financials**: La validez comercial y tarifas del `external_plan_id` son gobernadas por el microservicio financiero; este módulo almacena el identificador para vincular el contrato.
