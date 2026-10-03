# Funeral Homes Platform - Microservicio de Customers (`fh.api.customer`)

Este repositorio contiene la implementación del microservicio de **Customers** para la plataforma **Funeral Homes Project**, diseñado siguiendo principios de **Domain-Driven Design (DDD)**, arquitectura en capas/módulos y comunicación orientada a eventos (*Event-Driven Architecture*) [1, 2, 4].

---

## Propósito del Microservicio

El microservicio de **Customers** (`fh.api.customer` y `fh.db.customer`) es el único responsable de la administración demográfica, relacional y de jerarquía de los clientes y beneficiarios cubiertos por los planes funerarios [5, 6, 7].

### Responsabilidades Clave
* **Gestión de Cuentas / Titulares**: Clientes persona natural (B2C) o empresas (B2B) [6].
* **Control de Suscripciones**: Registro liviano de la afiliación al plan y control del cupo máximo (`max_beneficiaries`).
* **Sujetos Cubiertos**: Administración de miembros humanos (`HUMAN`) y mascotas (`PET`), calculando la **edad derivada** a partir de la fecha de nacimiento [5].
* **Auditoría y Eventos**: Registro de novedades y publicación asíncrona en **Apache Kafka** para el cálculo de cuotas en **Financials** [2, 7, 8].

---

## Arquitectura de Módulos (DDD)

El microservicio está estructurado internamente en **5 Módulos Bounded**, cada uno aislado con su propia separación DDD (`domain`, `application`, `infrastructure`, `api`):

```text
src/
├── modules/
│   ├── customer/                   # Módulo 1: Gestión de Titulares / Cuentas
│   │   ├── domain/                 # Entidad Customer, Reglas de Titularidad (B2C/B2B)
│   │   ├── application/            # Casos de uso: CreateCustomer, UpdateCustomerDemographics
│   │   ├── infrastructure/         # Repositorio JPA/SQL (fh.db.customer)
│   │   └── api/                    # Controladores REST para Titulares
│   │
│   ├── customer-plans/             # Módulo 2: Suscripción y Cupos
│   │   ├── domain/                 # Entidad Subscription, Control de Cupo (max_beneficiaries)
│   │   ├── application/            # Casos de uso: SubscribeCustomer, CancelSubscription
│   │   ├── infrastructure/         # Repositorio de Suscripciones
│   │   └── api/                    # Controladores REST de Suscripciones
│   │
│   ├── beneficiaries/              # Módulo 3: Sujetos Cubiertos (Personas/Mascotas)
│   │   ├── domain/                 # Entidad Member, Agregado Beneficiary, Edad Derivada
│   │   ├── application/            # Casos de uso: AddBeneficiary, RemoveBeneficiary
│   │   ├── infrastructure/         # Repositorio de Miembros y Beneficiarios
│   │   └── api/                    # Controladores REST de Beneficiarios
│   │
│   ├── audit/                      # Módulo 4: Auditoría e Historial de Novedades
│   │   ├── domain/                 # Entidad AuditLog, Reglas de Trazabilidad
│   │   ├── application/            # Casos de uso: GetSubscriptionHistory, RecordAudit
│   │   ├── infrastructure/         # Persistencia de Logs de Auditoría
│   │   └── api/                    # Consulta de Historial
│   │
│   ├── messaging/                  # Módulo 5 (FALTANTE): Integraciones y Eventos
│   │   ├── domain/                 # Definición de Eventos de Dominio (BeneficiaryAddedEvent)
│   │   ├── application/            # Manejadores de Eventos (Event Handlers)
│   │   └── infrastructure/         # Productor de Apache Kafka, Cliente Auth (fh.api.identity)
│   │
│   └── shared/                     # Kernel Compartido
│       ├── domain/                 # Value Objects: DocumentId, Address, ContactInfo
│       └── infrastructure/         # Configuración de BD, Middlewares de Seguridad
```

---

## Esquema de Base de Datos (`fh.db.customer`)

El microservicio utiliza una base de datos relacional dedicada [4, 7]:

```sql
-- 1. Tabla de Clientes Titulares / Cuentas
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

Este microservicio emite eventos asíncronos cuando ocurren cambios en los beneficiarios [2, 8]:

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
  * **Financials (`fh.api.financial`)**: Recalcula el costo de la cuota periódica del plan [7, 8].
  * **Notifications (`fh.api.notification`)**: Genera la alerta/correo de confirmación al titular [11, 12].

---

## Endpoints REST Principales (`fh.api.customer`)

### Módulo `customer`
* `POST /api/v1/customers` -> Crear cliente (Persona natural / Empresa)
* `GET /api/v1/customers/{id}` -> Consultar ficha del cliente
* `PUT /api/v1/customers/{id}` -> Actualizar datos demográficos/contacto

### Módulo `customer-plans`
* `POST /api/v1/subscriptions` -> Crear suscripción a un plan
* `GET /api/v1/customers/{customerId}/subscriptions` -> Consultar suscripciones del cliente

### Módulo `beneficiaries`
* `POST /api/v1/subscriptions/{subscriptionId}/beneficiaries` -> Agregar beneficiario (humano/mascota)
* `GET /api/v1/subscriptions/{subscriptionId}/beneficiaries` -> Listar grupo cubierto
* `DELETE /api/v1/subscriptions/{subscriptionId}/beneficiaries/{memberId}` -> Retirar beneficiario

### Módulo `audit`
* `GET /api/v1/subscriptions/{subscriptionId}/audit-log` -> Consultar historial de novedades

---

## Requisitos e Instalación

1. **Prerrequisitos**:
   * Docker & Docker Compose
   * Database: PostgreSQL / MySQL (`fh.db.customer`)
   * Message Broker: Apache Kafka
2. **Variables de Entorno (`.env`)**:
   ```env
   PORT=8080
   DB_HOST=localhost
   DB_PORT=5432
   DB_NAME=fh_db_customer
   DB_USER=postgres
   DB_PASS=secret
   KAFKA_BROKERS=localhost:9092
   IDENTITY_SERVICE_URL=http://localhost:8081
   ```
3. **Ejecución Local**:
   ```bash
   # Clonar e instalar dependencias
   npm install # o dotnet restore / ./gradlew build
   
   # Iniciar base de datos y Kafka
   docker-compose up -d
   
   # Iniciar el microservicio
   npm run start:dev
   ```
