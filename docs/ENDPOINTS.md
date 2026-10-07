# Endpoints del microservicio Customers

- **URL base (perfil https):** `https://localhost:7035` · **Swagger:** `/swagger` · **Salud:** `GET /health`
- **Prueba automática de todo:** `.\scripts\smoke-test.ps1`
- Los enums se envían como texto (`"Individual"`, `"Spouse"`…).

## Códigos de error

| Código | Cuándo |
|---|---|
| `400` | Datos inválidos (validación de FluentValidation) o regla de dominio incumplida |
| `404` | El recurso no existe |
| `409` | Conflicto: documento duplicado, cupo lleno, beneficiario ya retirado |
| `422` | La suscripción existe pero no está activa |

Un `400` de validación responde `{ "title": ..., "errors": { "Campo": ["mensaje"] } }`; los demás, `{ "message": "...", "code": "..." }`.

---

## Customers · `/api/v1/customers`

| Método | Ruta | Respuesta |
|---|---|---|
| `POST` | `/api/v1/customers` | 201 · 400 · 409 |
| `GET` | `/api/v1/customers?status=Active` | 200 (`status` opcional) |
| `GET` | `/api/v1/customers/{id}` | 200 · 404 |
| `GET` | `/api/v1/customers/by-identification?type=CC&number=123` | 200 · 404 |
| `PUT` | `/api/v1/customers/{id}` | 200 · 400 · 404 |
| `PATCH` | `/api/v1/customers/{id}/status` | 204 · 404 |

```jsonc
// POST /api/v1/customers
{ "customerType": "Individual", "name": "Ana Gómez", "identificationType": "CC", "identificationNumber": "1010101010",
  "email": "ana@correo.com", "phone": "+573001112233", "address": "Calle 1 # 2-3" }   // email, phone, address opcionales

// PUT /api/v1/customers/{id}   (no cambia el documento)
{ "name": "Ana Gómez R.", "email": "nuevo@correo.com", "phone": "+573009998877", "address": "Calle 2" }

// PATCH /api/v1/customers/{id}/status      status: Active | Inactive | Suspended
{ "status": "Suspended" }
```

Reglas: la clave de negocio es **tipo + número** de documento (el mismo número con otro tipo es otro cliente). Tipo y número se guardan en mayúsculas y el correo en minúsculas (value objects `DocumentId` y `ContactInfo`).

## Subscriptions · `/api/v1/subscriptions`

| Método | Ruta | Respuesta |
|---|---|---|
| `POST` | `/api/v1/subscriptions` | 201 · 400 · 404 (cliente) |
| `GET` | `/api/v1/subscriptions/{id}` | 200 · 404 |
| `GET` | `/api/v1/subscriptions/{id}/capacity` | 200 · 404 |
| `GET` | `/api/v1/customers/{customerId}/subscriptions` | 200 |
| `PATCH` | `/api/v1/subscriptions/{id}/max-beneficiaries` | 204 · 400 · 404 · 422 |
| `PATCH` | `/api/v1/subscriptions/{id}/cancel` | 204 · 404 · 422 |

```jsonc
// POST /api/v1/subscriptions
{ "customerId": "<id del cliente>", "externalPlanId": "99999999-9999-9999-9999-999999999999", "maxBeneficiaries": 4, "startDate": "2026-10-07" }

// PATCH /api/v1/subscriptions/{id}/max-beneficiaries
{ "maxBeneficiaries": 5 }
```

## Beneficiaries · `/api/v1/subscriptions/{subscriptionId}/beneficiaries`

| Método | Ruta | Respuesta |
|---|---|---|
| `POST` | `.../beneficiaries` | 201 · 400 · 404 · 409 · 422 |
| `GET` | `.../beneficiaries?status=Active` | 200 · 404 (`status`: Active \| Removed) |
| `PUT` | `.../beneficiaries/{memberId}` | 200 · 400 · 404 · 409 |
| `DELETE` | `.../beneficiaries/{memberId}` | 204 · 404 · 409 |

```jsonc
// POST
{ "subjectType": "Human", "firstName": "Laura", "lastName": "Gómez", "birthDate": "1990-05-10",
  "beneficiaryType": "Associated", "relationshipType": "Spouse",
  "identificationType": "CC", "identificationNumber": "55555", "email": "laura@correo.com", "phone": "+573105559876" }

// PUT  (no cambia subjectType ni tipos de beneficiario)
{ "firstName": "Laura Sofía", "lastName": "Gómez Pérez", "birthDate": "1990-05-10",
  "identificationType": "CC", "identificationNumber": "55555", "email": "nuevo@correo.com", "phone": "+573105550000" }
```

Reglas: el documento es opcional, pero **tipo y número van juntos** (si falta uno → 400). Una mascota (`subjectType: "Pet"`) debe tener `relationshipType: "Pet"`. No se repite el mismo documento activo en una suscripción (409) ni se supera el cupo (409). Cada alta, cambio o retiro genera un registro de auditoría mediante eventos de dominio.

## Audit

| Método | Ruta | Respuesta |
|---|---|---|
| `GET` | `/api/v1/subscriptions/{subscriptionId}/audit-log` | 200 · 404 |
| `GET` | `/api/v1/members/{memberId}/audit-log` | 200 · 404 |
