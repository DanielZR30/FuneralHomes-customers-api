# Módulo de Auditoría (`FH.Modules.Audit`)

Este módulo forma parte del microservicio **Customers** (`fh.api.customer`) y está diseñado para garantizar la **trazabilidad inmutable de novedades** y actuar como la base del patrón **Transactional Outbox** para la publicación confiable de eventos asíncronos hacia Apache Kafka.

---

## 1. Responsabilidades del Módulo

El módulo `FH.Modules.Audit` tiene la responsabilidad exclusiva de registrar y consultar todo cambio que impacte el grupo de beneficiarios y las suscripciones:

1. **Registro Inmutable de Novedades (Audit Trail)**:
   - Registrar de forma no modificable cada acción sobre beneficiarios:
     - `BENEFICIARY_ADDED`: Incorporación de un nuevo beneficiario a la suscripción.
     - `BENEFICIARY_REMOVED`: Desafiliación/retiro de un miembro.
     - `BENEFICIARY_UPDATED`: Modificación de datos del miembro o parentesco.
2. **Soporte para el Patrón Transactional Outbox**:
   - Administrar la bandera `event_published` (`FALSE` al crear el log; `TRUE` una vez despachado a Kafka con confirmación ack).
   - Garantizar la consistencia dual: los cambios de base de datos y la intención de evento se persisten en la misma transacción relacional ACID.
3. **Consulta Histórica y Cumplimiento Normativo**:
   - Exponer consultas ordenadas cronológicamente para que operadores, auditores y sistemas externos reconstruyan el historial de cobertura de una póliza.
   - Brindar auditoría por sujeto cubierto (`member_id`) a través de diferentes pólizas o periodos.
4. **Garantía de Inmutabilidad**:
   - Prohibir operaciones de eliminación (`DELETE`) o edición (`UPDATE`) sobre los registros de auditoría, excepto la actualización del estado de publicación (`event_published`).

---

## 2. Historias de Usuario (User Stories)

### HU-AUD-01: Registro Automático de Novedad de Beneficiario
* **Como** Sistema de Gestión de Clientes,  
* **Quiero** registrar automáticamente un log de auditoría cada vez que se agrega, retira o modifica un beneficiario,  
* **Para** mantener una bitácora inalterable y preparar el evento asíncrono para el broker de mensajería.

#### Criterios de Aceptación (Gherkin):
* **Escenario 1: Registro de log ante adición de beneficiario**
  - **Dado** que se añade exitosamente un beneficiario a una suscripción en la transacción principal,
  - **Cuando** se ejecuta el caso de uso,
  - **Entonces** se inserta un registro en `beneficiary_audit_log` con `action = BENEFICIARY_ADDED`, `subscription_id`, `member_id`, `event_published = FALSE` y `created_at = UtcNow`.
* **Escenario 2: Registro de log ante retiro de beneficiario**
  - **Dado** que se desafilia a un miembro de la suscripción,
  - **Cuando** se procesa la desvinculación,
  - **Entonces** se inserta un registro con `action = BENEFICIARY_REMOVED`, `event_published = FALSE` y marca temporal actual.

#### Tareas Técnicas:
- [ ] Implementar `RecordAuditLogCommand` o consumir eventos de dominio en memoria (`IDomainEventHandler<T>` y `IDomainEventDispatcher` propios).
- [ ] Mapear la entidad `BeneficiaryAuditLog` en la transacción activa.
- [ ] Asegurar que `event_published` inicie en `false`.

---

### HU-AUD-02: Consulta de Historial de Novedades de una Suscripción
* **Como** Agente de Servicio al Cliente o Auditor,  
* **Quiero** consultar la cronología completa de novedades de una suscripción funeraria,  
* **Para** resolver reclamos de cobertura, confirmar quién retiró a un familiar o validar fechas de afiliación.

#### Criterios de Aceptación:
* **Escenario 1: Historial cronológico obtenido**
  - **Dado** un `subscriptionId` existente que registra novedades,
  - **Cuando** se invoca `GET /api/v1/subscriptions/{subscriptionId}/audit-log`,
  - **Entonces** el sistema retorna la lista de logs ordenada de forma descendente por `created_at`, indicando el miembro afectado, tipo de novedad y fecha/hora exacta.
* **Escenario 2: Suscripción sin movimientos**
  - **Dado** una suscripción que no tiene novedades registradas,
  - **Cuando** se realiza la consulta,
  - **Entonces** se devuelve HTTP 200 OK con un arreglo vacío.

---

### HU-AUD-03: Actualización de Estado de Publicación de Eventos
* **Como** Worker de Publicación de Mensajería (`OutboxDispatcherService`),  
* **Quiero** marcar un log de auditoría como publicado (`event_published = TRUE`),  
* **Para** evitar la republicación de mensajes duplicados hacia Apache Kafka.

#### Criterios de Aceptación:
* **Escenario 1: Marcado exitoso tras confirmación del Broker**
  - **Dado** un registro de auditoría con `event_published = FALSE` que fue enviado a Kafka y recibió ACK,
  - **Cuando** el despachador ejecuta `MarkAsPublished(id)`,
  - **Entonces** el registro en base de datos pasa a `event_published = TRUE`.

---

### HU-AUD-04: Consulta de Novedades Pendientes de Despacho (Outbox Pull)
* **Como** Servicio en Segundo Plano (Outbox Worker),  
* **Quiero** consultar por lotes los registros de auditoría no publicados (`event_published == false`),  
* **Para** enviarlos secuencialmente a los tópicos de Kafka.

#### Criterios de Aceptación:
* **Escenario 1: Obtención de lote no publicado**
  - **Dado** que existen 5 eventos con `event_published = FALSE`,
  - **Cuando** el worker ejecuta la consulta con límite de lote (e.g. 50 registros),
  - **Entonces** se retornan los 5 registros en orden ascendente por `created_at` para mantener el orden cronológico estricto de eventos.

---

## 3. Explicación Técnica y Arquitectura

### 3.1 Estructura Interna del Módulo (DDD)
Ubicación: `carpeta Audit en Domain / Application / Persistence`

```text
FH.Modules.Audit/
├── domain/                      # Lógica inmutable de auditoría
│   ├── Entities/                # BeneficiaryAuditLog (Entity<Guid>)
│   ├── Enums/                   # AuditAction (BeneficiaryAdded, BeneficiaryRemoved, BeneficiaryUpdated)
│   └── Repositories/            # IBeneficiaryAuditLogRepository
│
├── application/                 # Casos de uso y Event Handlers
│   ├── Commands/
│   │   ├── RecordAuditLog/      # Command, Handler
│   │   └── MarkAsPublished/     # Command, Handler
│   ├── Queries/
│   │   ├── GetSubscriptionAuditLog/
│   │   └── GetUnpublishedAuditLogs/ # Usado por el Outbox Worker
│   └── EventHandlers/           # Handlers que escuchan BeneficiaryAddedDomainEvent
│
├── infrastructure/              # Repositorio EF Core
│   └── Repositories/            # BeneficiaryAuditLogRepository
│
└── README.md                    # Este archivo
```

### 3.2 Modelo de Base de Datos (`beneficiary_audit_log`)

```sql
CREATE TABLE beneficiary_audit_log (
    id UUID PRIMARY KEY,
    subscription_id UUID NOT NULL REFERENCES customer_subscriptions(id),
    member_id UUID NOT NULL REFERENCES members(id),
    action VARCHAR(30) NOT NULL, -- BENEFICIARY_ADDED, BENEFICIARY_REMOVED, BENEFICIARY_UPDATED
    event_published BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Índices de alto rendimiento
CREATE INDEX idx_audit_subscription ON beneficiary_audit_log(subscription_id, created_at DESC);
CREATE INDEX idx_audit_unpublished ON beneficiary_audit_log(event_published) WHERE event_published = FALSE;
CREATE INDEX idx_audit_member ON beneficiary_audit_log(member_id);
```

### 3.3 El Patrón Transactional Outbox
1. **Transacción de Negocio (ACID)**:
   Al ejecutar `AddBeneficiaryCommand`:
   ```csharp
   using var transaction = await dbContext.Database.BeginTransactionAsync();
   // 1. Guardar miembro y beneficiario
   dbContext.Members.Add(member);
   dbContext.Beneficiaries.Add(beneficiary);
   
   // 2. Guardar registro en el Outbox / Audit Log
   var auditLog = new BeneficiaryAuditLog(
       Guid.NewGuid(),
       subscriptionId,
       member.Id,
       AuditAction.BeneficiaryAdded,
       eventPublished: false
   );
   dbContext.BeneficiaryAuditLogs.Add(auditLog);
   
   await dbContext.SaveChangesAsync();
   await transaction.CommitAsync();
   ```
2. **Despacho Desacoplado**:
   El módulo de mensajería (`FH.Modules.Messaging`) procesa los registros con `event_published = false` y los publica a Kafka sin bloquear la respuesta HTTP al usuario.

### 3.4 Contratos y Endpoints REST

| Método | Endpoint | Descripción | Respuesta Exitosa |
|---|---|---|---|
| `GET` | `/api/v1/subscriptions/{subscriptionId}/audit-log` | Historial de auditoría de una suscripción | `200 OK` |
| `GET` | `/api/v1/members/{memberId}/audit-log` | Historial de movimientos de un miembro específico | `200 OK` |

#### Payload de Respuesta (`GET .../audit-log`):
```json
[
  {
    "id": "11111111-2222-3333-4444-555555555555",
    "subscriptionId": "987e6543-e21b-12d3-a456-426614174111",
    "memberId": "456e7890-e89b-12d3-a456-426614174222",
    "action": "BeneficiaryAdded",
    "eventPublished": true,
    "createdAt": "2026-10-03T10:15:30Z"
  }
]
```
