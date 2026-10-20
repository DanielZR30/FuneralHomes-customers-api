# Customers

## Create

```bash
curl -X 'POST' \
  'https://localhost:7035/api/v1/customers' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d '{
  "customerType": "Individual",
  "name": "Ana María Gómez",
  "identificationType": "CC",
  "identificationNumber": "1010101010",
  "email": "ana@correo.com",
  "phone": "3001234567",
  "address": "Calle 1"
}'
```

## List

```bash
curl -X 'GET' \
  'https://localhost:7035/api/v1/customers?status=Active' \
  -H 'accept: text/plain'
```

## Get by id

```bash
curl -X 'GET' \
  'https://localhost:7035/api/v1/customers/11111111-1111-1111-1111-111111111111' \
  -H 'accept: text/plain'
```

## Get by identification

```bash
curl -X 'GET' \
  'https://localhost:7035/api/v1/customers/by-identification?type=CC&number=1010101010' \
  -H 'accept: text/plain'
```

## Update

```bash
curl -X 'PUT' \
  'https://localhost:7035/api/v1/customers/11111111-1111-1111-1111-111111111111' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d '{
  "name": "Ana María",
  "email": "nuevo@correo.com",
  "phone": "3009999999",
  "address": "Av. 1"
}'
```

## Change status

```bash
curl -X 'PATCH' \
  'https://localhost:7035/api/v1/customers/11111111-1111-1111-1111-111111111111/status' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d '{
  "status": "Suspended"
}'
```

# Subscriptions

## Create

```bash
curl -X 'POST' \
  'https://localhost:7035/api/v1/subscriptions' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d '{
  "customerId": "11111111-1111-1111-1111-111111111111",
  "externalPlanId": "99999999-9999-9999-9999-999999999999",
  "maxBeneficiaries": 4,
  "startDate": "2026-10-01"
}'
```

## Get by id

```bash
curl -X 'GET' \
  'https://localhost:7035/api/v1/subscriptions/22222222-2222-2222-2222-222222222222' \
  -H 'accept: text/plain'
```

## Get capacity

```bash
curl -X 'GET' \
  'https://localhost:7035/api/v1/subscriptions/22222222-2222-2222-2222-222222222222/capacity' \
  -H 'accept: text/plain'
```

## List by customer

```bash
curl -X 'GET' \
  'https://localhost:7035/api/v1/customers/11111111-1111-1111-1111-111111111111/subscriptions' \
  -H 'accept: text/plain'
```

## Update max beneficiaries

```bash
curl -X 'PATCH' \
  'https://localhost:7035/api/v1/subscriptions/22222222-2222-2222-2222-222222222222/max-beneficiaries' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d '{
  "maxBeneficiaries": 5
}'
```

## Cancel

```bash
curl -X 'PATCH' \
  'https://localhost:7035/api/v1/subscriptions/22222222-2222-2222-2222-222222222222/cancel' \
  -H 'accept: text/plain'
```

# Beneficiaries

## Create

```bash
curl -X 'POST' \
  'https://localhost:7035/api/v1/subscriptions/22222222-2222-2222-2222-222222222222/beneficiaries' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d '{
  "subjectType": "Human",
  "firstName": "Luis",
  "lastName": "Pérez",
  "birthDate": "1990-01-01",
  "beneficiaryType": "Associated",
  "relationshipType": "Spouse",
  "identificationType": "CC",
  "identificationNumber": "123"
}'
```

## List

```bash
curl -X 'GET' \
  'https://localhost:7035/api/v1/subscriptions/22222222-2222-2222-2222-222222222222/beneficiaries?status=Active' \
  -H 'accept: text/plain'
```

## Update

```bash
curl -X 'PUT' \
  'https://localhost:7035/api/v1/subscriptions/22222222-2222-2222-2222-222222222222/beneficiaries/33333333-3333-3333-3333-333333333333' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d '{
  "firstName": "Laura Sofía",
  "lastName": "Gómez Pérez",
  "birthDate": "1990-05-10",
  "identificationType": "CC",
  "identificationNumber": "55555",
  "email": "nuevo@correo.com",
  "phone": "+573105550000"
}'
```

## Remove

```bash
curl -X 'DELETE' \
  'https://localhost:7035/api/v1/subscriptions/22222222-2222-2222-2222-222222222222/beneficiaries/33333333-3333-3333-3333-333333333333' \
  -H 'accept: text/plain'
```

## Reactivate

```bash
curl -X 'PATCH' \
  'https://localhost:7035/api/v1/subscriptions/22222222-2222-2222-2222-222222222222/beneficiaries/33333333-3333-3333-3333-333333333333/reactivate' \
  -H 'accept: text/plain'
```

# Audit

## Get by subscription

```bash
curl -X 'GET' \
  'https://localhost:7035/api/v1/subscriptions/22222222-2222-2222-2222-222222222222/audit-log' \
  -H 'accept: text/plain'
```

## Get by member

```bash
curl -X 'GET' \
  'https://localhost:7035/api/v1/members/33333333-3333-3333-3333-333333333333/audit-log' \
  -H 'accept: text/plain'
```
