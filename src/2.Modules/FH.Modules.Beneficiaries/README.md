# Módulo de Beneficiarios (`FH.Modules.Beneficiaries`)

Este módulo forma parte del microservicio **Customers** (`fh.api.customer`) y es el núcleo operativo que gestiona a los **sujetos cubiertos** (personas naturales y mascotas) y sus vínculos contractuales como beneficiarios de una suscripción de previsión exequial.

---

## 1. Responsabilidades del Módulo

El módulo `FH.Modules.Beneficiaries` es responsable de la administración de la cobertura familiar y de dependientes:

1. **Gestión Unificada de Miembros (`Member`)**:
   - Registro de sujetos individuales cubiertos: seres humanos (`HUMAN`) y mascotas (`PET`).
   - Normalización de datos personales (nombre, apellidos, documento, teléfono, email).
   - Para mascotas: validación de nombre y registro de fechas sin obligatoriedad de documento de identidad humano.
2. **Cálculo de Edad Derivada (`derived_age`)**:
   - Cálculo automático de la edad en años cumplidos a partir de la fecha de nacimiento (`birth_date`).
   - Garantizar que este valor esté siempre actualizado, ya que constituye la variable base para la tarificación actuarial en el microservicio de **Financials**.
3. **Vínculo de Cobertura (`Beneficiary`)**:
   - Asignación de roles: Beneficiario Principal (`PRINCIPAL`) o Asociado (`ASSOCIATED`).
   - Clasificación del parentesco (`TITULAR`, `SPOUSE`, `CHILD`, `PARENT`, `EMPLOYEE`, `PET`, `OTHER`).
   - Gestión del alta (`joined_at`, estado `Active`) y baja (`removed_at`, estado `Removed`).
4. **Control de Invariantes de Póliza**:
   - Asegurar que la cantidad de beneficiarios activos nunca sobrepase el `max_beneficiaries` configurado en la suscripción.
   - Evitar la adición duplicada del mismo miembro en estado activo dentro de la misma suscripción.
5. **Generación de Eventos de Dominio**:
   - Emitir `BeneficiaryAddedDomainEvent` y `BeneficiaryRemovedDomainEvent` para que los módulos de Auditoría y Mensajería (Kafka) propaguen las novedades.

---

## 2. Historias de Usuario (User Stories)

### HU-BEN-01: Incorporación de Beneficiario Humano a una Suscripción
* **Como** Titular del Plan o Asesor de Afiliaciones,  
* **Quiero** afiliar a un familiar o dependiente humano a mi suscripción activa,  
* **Para** garantizarle los servicios de cobertura exequial en caso de fallecimiento.

#### Criterios de Aceptación (Gherkin):
* **Escenario 1: Alta exitosa de beneficiario humano**
  - **Dado** una suscripción activa con al menos un cupo disponible (`activos < max_beneficiaries`),
  - **Cuando** se envían los datos del miembro con `SubjectType = Human`, fecha de nacimiento válida y parentesco,
  - **Entonces** el sistema calcula la edad derivada (`derived_age = UtcNow.Year - birthDate.Year`), crea el registro en `members`, crea el vínculo en `beneficiaries` con `status = Active` y `joined_at = UtcNow`, emite el evento de adición y retorna HTTP 201 Created.
* **Escenario 2: Cupo de suscripción agotado**
  - **Dado** una suscripción cuyo total de beneficiarios activos es igual a `max_beneficiaries`,
  - **Cuando** se intenta afiliar a un nuevo beneficiario,
  - **Entonces** la solicitud se rechaza con HTTP 409 Conflict o 422 Unprocessable Entity indicando cupo agotado.
* **Escenario 3: Fecha de nacimiento futura o inválida**
  - **Dado** una fecha de nacimiento posterior a la fecha actual,
  - **Cuando** se valida la solicitud,
  - **Entonces** el sistema rechaza el comando con HTTP 400 Bad Request.

#### Tareas Técnicas:
- [ ] Implementar `AddBeneficiaryCommand` y validador FluentValidation.
- [ ] Implementar `AgeCalculationService` o método en entidad `Member` para calcular `derived_age`.
- [ ] Validar disponibilidad de cupo mediante `CustomerSubscription`.
- [ ] Persistir `Member` y `Beneficiary` en transacción atómica.
- [ ] Disparar evento de dominio `BeneficiaryAddedDomainEvent`.
- [ ] Exponer endpoint `POST /api/v1/subscriptions/{subscriptionId}/beneficiaries`.

---

### HU-BEN-02: Incorporación de Mascota (`PET`) como Beneficiario
* **Como** Titular de una Póliza Pet-Friendly,  
* **Quiero** afiliar a mi mascota a la suscripción indicando su nombre y fecha de nacimiento/adopción,  
* **Para** que cuente con auxilio exequial y servicios crematorios para mascotas.

#### Criterios de Aceptación:
* **Escenario 1: Alta exitosa de mascota**
  - **Dado** una suscripción activa con cupo disponible,
  - **Cuando** se envía la solicitud con `SubjectType = Pet`, nombre de la mascota y parentesco `PET`,
  - **Entonces** el sistema no exige documento de identidad, calcula la edad derivada de la mascota, vincula el beneficiario en estado `Active` y retorna HTTP 201 Created.
* **Escenario 2: Asignación de parentesco inconsistente**
  - **Dado** un `SubjectType = Pet` pero con un parentesco humano (e.g. `SPOUSE`),
  - **Cuando** se valida la solicitud,
  - **Entonces** se rechaza con HTTP 400 Bad Request indicando incoherencia en el tipo de relación.

---

### HU-BEN-03: Consulta del Grupo de Beneficiarios Cubiertos
* **Como** Titular del Plan o Agente de Operaciones,  
* **Quiero** consultar la lista de todos los beneficiarios registrados en una suscripción,  
* **Para** verificar quiénes están cubiertos actualmente, sus edades derivadas y sus parentescos.

#### Criterios de Aceptación:
* **Escenario 1: Listado de beneficiarios activos**
  - **Dado** un `subscriptionId` válido,
  - **Cuando** se consulta `GET /api/v1/subscriptions/{subscriptionId}/beneficiaries`,
  - **Entonces** se retorna la lista completa con información del miembro (nombre, tipo, edad derivada) y del vínculo (parentesco, fecha de ingreso, estado).
* **Escenario 2: Filtro por estado**
  - **Dado** un parámetro `status=Active` o `status=Removed`,
  - **Cuando** se realiza la consulta,
  - **Entonces** solo se retornan los beneficiarios que cumplan dicho estado.

---

### HU-BEN-04: Retiro / Desafiliación de un Beneficiario
* **Como** Titular del Plan,  
* **Quiero** desafiliar a un beneficiario de mi suscripción,  
* **Para** liberar un cupo en mi póliza y ajustar los costos de cobro.

#### Criterios de Aceptación:
* **Escenario 1: Retiro exitoso**
  - **Dado** un beneficiario con estado `Active` en la suscripción indicada,
  - **Cuando** se envía la solicitud `DELETE /api/v1/subscriptions/{subscriptionId}/beneficiaries/{memberId}`,
  - **Entonces** el estado del beneficiario cambia a `Removed`, se establece `removed_at = UtcNow`, se libera el cupo en la suscripción, se emite `BeneficiaryRemovedDomainEvent` y se retorna HTTP 200 OK o 204 No Content.
* **Escenario 2: Beneficiario ya retirado previamente**
  - **Dado** un beneficiario que ya tiene estado `Removed`,
  - **Cuando** se intenta volver a retirar,
  - **Entonces** se rechaza la solicitud indicando que el miembro ya no está activo en dicha suscripción.

---

### HU-BEN-05: Verificación de Duplicidad de Beneficiario
* **Como** Sistema de Registro,  
* **Quiero** impedir que un mismo miembro se afilie más de una vez en forma activa a la misma suscripción,  
* **Para** evitar duplicidad de cobros y colisión de coberturas.

#### Criterios de Aceptación:
* **Escenario 1: Detección de duplicado**
  - **Dado** un miembro ya activo en la suscripción X,
  - **Cuando** se intenta afiliar nuevamente el mismo `memberId` a la suscripción X,
  - **Entonces** el sistema responde con HTTP 409 Conflict impidiendo el registro.

---

## 3. Explicación Técnica y Arquitectura

### 3.1 Estructura Interna del Módulo (DDD)
Ubicación: `src/2.Modules/FH.Modules.Beneficiaries`

```text
FH.Modules.Beneficiaries/
├── domain/                      # Entidades y servicios de dominio
│   ├── Entities/                # Member, Beneficiary
│   ├── Enums/                   # SubjectType, BeneficiaryType, RelationshipType, BeneficiaryStatus
│   ├── Services/                # AgeCalculationService
│   ├── Events/                  # BeneficiaryAddedDomainEvent, BeneficiaryRemovedDomainEvent
│   ├── Repositories/            # IMemberRepository, IBeneficiaryRepository
│   └── Exceptions/              # BeneficiaryCapacityExceededException, DuplicateBeneficiaryException
│
├── application/                 # Casos de uso
│   ├── Commands/
│   │   ├── AddBeneficiary/      # Command, Handler, Validator
│   │   └── RemoveBeneficiary/   # Command, Handler
│   ├── Queries/
│   │   ├── GetBeneficiariesBySubscription/
│   │   └── GetMemberById/
│   └── DTOs/                    # BeneficiaryResponseDto, MemberDto, AddBeneficiaryRequestDto
│
├── infrastructure/              # Repositorios EF Core
│   └── Repositories/            # MemberRepository, BeneficiaryRepository
│
└── README.md                    # Este archivo
```

### 3.2 Modelo de Base de Datos (`members` y `beneficiaries`)

```sql
-- 1. Sujetos Cubiertos (Personas / Mascotas)
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

-- 2. Beneficiarios Vinculados a la Suscripción
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

CREATE INDEX idx_beneficiaries_subscription ON beneficiaries(subscription_id);
CREATE INDEX idx_beneficiaries_member ON beneficiaries(member_id);
```

### 3.3 Cálculo de Edad Derivada
La edad se deriva de manera estandarizada mediante la siguiente lógica de dominio:
```csharp
public static int CalculateAge(DateTime birthDate, DateTime? currentDate = null)
{
    var today = currentDate ?? DateTime.UtcNow.Date;
    var age = today.Year - birthDate.Year;
    if (birthDate.Date > today.AddYears(-age))
    {
        age--;
    }
    return Math.Max(0, age);
}
```

### 3.4 Contratos y Endpoints REST

| Método | Endpoint | Descripción | Respuesta Exitosa | Códigos de Error |
|---|---|---|---|---|
| `POST` | `/api/v1/subscriptions/{subscriptionId}/beneficiaries` | Incorporar beneficiario (humano o mascota) | `201 Created` | `400`, `404`, `409` |
| `GET` | `/api/v1/subscriptions/{subscriptionId}/beneficiaries` | Listar grupo cubierto de una suscripción | `200 OK` | `404` |
| `DELETE` | `/api/v1/subscriptions/{subscriptionId}/beneficiaries/{memberId}` | Desafiliar beneficiario | `200 OK` / `204` | `404`, `409` |

#### Payload de Ejemplo (`POST .../beneficiaries`):
```json
{
  "subjectType": "Human",
  "firstName": "María",
  "lastName": "Gómez",
  "identificationType": "CC",
  "identificationNumber": "52987654",
  "birthDate": "1961-05-14",
  "beneficiaryType": "Associated",
  "relationshipType": "Parent",
  "email": "maria.gomez@example.com",
  "phone": "+573105559876"
}
```
O para una mascota:
```json
{
  "subjectType": "Pet",
  "firstName": "Max",
  "birthDate": "2022-03-10",
  "beneficiaryType": "Associated",
  "relationshipType": "Pet"
}
```
---

## 4. Estado de implementación (Entrega 1)

**Implementado**
- Alta de beneficiario (humano y mascota), retiro y listado con filtro por estado, como Commands/Queries con MediatR y `Result<T>`.
- Persistencia con EF Core y PostgreSQL mediante `IBeneficiaryRepository` y `IUnitOfWork`: el `Member` y el `Beneficiary` se guardan en una sola transacción.
- Reglas de dominio dentro de las entidades, lanzadas como `BusinessRuleException`: la mascota solo admite parentesco `Pet`, no se puede retirar dos veces, la fecha de nacimiento no puede ser futura y el nombre es obligatorio.
- Suscripción existente y activa, y cupo máximo, validados en el handler de alta.
- Duplicados (HU-BEN-05) por tipo y número de documento entre beneficiarios activos de la misma suscripción.
- Eventos de dominio `BeneficiaryAddedDomainEvent` y `BeneficiaryRemovedDomainEvent`, emitidos por `Beneficiary`.

**Diferencias respecto al diseño inicial**
- `Member`, `Beneficiary`, los eventos, `DerivedAgeCalculator` y `BusinessRuleException` viven en `FH.Shared`, no dentro del módulo.
- No hay `IMemberRepository`, validadores FluentValidation ni consulta `GetMemberById`: las validaciones están en el dominio y en los handlers.
- Los duplicados se detectan por documento y no por `memberId`, porque cada alta crea un `Member` nuevo.

**Límites conocidos y pendiente**
- Las mascotas no tienen documento, por lo que no se verifica duplicidad para ellas.
- El registro en `beneficiary_audit_log` está a cargo del módulo Audit, que debe escuchar los eventos de dominio.
- La actualización de datos de un beneficiario (`BENEFICIARY_UPDATED`) no está implementada.