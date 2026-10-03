# Arquitectura Modular del Microservicio Customers (`src/2.Modules`)

Este directorio contiene los **Módulos Bounded (DDD)** que estructuran el microservicio de **Customers** (`fh.api.customer`), siguiendo los lineamientos definidos en [`readme-customers-microservice.md`](../../readme-customers-microservice.md).

---

## Mapa de Módulos

```text
src/
├── 1.Api/                        # Host ejecutable (ASP.NET Core Web API)
├── 2.Modules/                    # Módulos de Dominio Bounded
│   ├── FH.Modules.Customer/      # Módulo 1: Titulares y Cuentas (B2C / B2B)
│   ├── FH.Modules.CustomerPlans/ # Módulo 2: Suscripciones y Control de Cupos
│   ├── FH.Modules.Beneficiaries/ # Módulo 3: Sujetos Cubiertos (Personas/Mascotas) y Parentescos
│   └── FH.Modules.Audit/         # Módulo 4: Bitácora Inmutable y Transactional Outbox
└── 3.Externals/                  # Componentes Transversales e Integraciones
    ├── FH.Shared/                # Entidades, DbContext y Value Objects compartidos
    └── FH.Modules.Messaging/     # Módulo 5: Integraciones y Publicador Kafka
```

---

## Documentación Específica por Módulo

Cada módulo cuenta con su propio `README.md` detallando **Responsabilidades**, **Historias de Usuario (HU)** con criterios de aceptación en Gherkin, y **Explicación Técnica**:

| Módulo | Enlace a Documentación | Propósito Principal |
|---|---|---|
| **Customer** | [`FH.Modules.Customer/README.md`](./FH.Modules.Customer/README.md) | Gestión de clientes persona natural y empresas, validación de identificación única y ciclo de vida de la cuenta. |
| **Customer Plans** | [`FH.Modules.CustomerPlans/README.md`](./FH.Modules.CustomerPlans/README.md) | Suscripción a planes de previsión funeraria, vigencias temporales y control de cupo máximo (`max_beneficiaries`). |
| **Beneficiaries** | [`FH.Modules.Beneficiaries/README.md`](./FH.Modules.Beneficiaries/README.md) | Administración de seres humanos y mascotas, cálculo automático de `derived_age` y relaciones de parentesco. |
| **Audit** | [`FH.Modules.Audit/README.md`](./FH.Modules.Audit/README.md) | Registro inmutable de altas y bajas, y almacenamiento de eventos para el patrón Transactional Outbox. |
| **Messaging** | [`../3.Externals/FH.Modules.Messaging/README.md`](../3.Externals/FH.Modules.Messaging/README.md) | Publicación de eventos en Apache Kafka para los microservicios de Financials y Notifications. |

---

## Flujo de Interacción entre Módulos

```text
[Cliente Titular] (FH.Modules.Customer)
       │
       ▼ (1. Se afilia a un Plan con cupo N)
[Suscripción] (FH.Modules.CustomerPlans)
       │
       ▼ (2. Se vinculan hasta N beneficiarios)
[Miembros / Beneficiarios] (FH.Modules.Beneficiaries)
       │
       ▼ (3. Se registra cada novedad)
[Log de Auditoría / Outbox] (FH.Modules.Audit)
       │
       ▼ (4. Worker despacha a Kafka)
[Broker Kafka] (FH.Modules.Messaging)
       │
       ├────► [fh.api.financial] (Recalcular cuota por edad/sujeto)
       └────► [fh.api.notification] (Enviar confirmación al titular)
```

---

## Principios de Diseño Aplicados
1. **Clean Architecture & DDD**: Separación estricta de responsabilidades entre `domain`, `application` e `infrastructure`.
2. **Transactional Outbox**: La persistencia de la novedad y el registro de auditoría ocurren en la misma transacción ACID, garantizando entrega garantizada sin riesgo de desincronización con Kafka.
3. **Bajo Acoplamiento**: Los módulos se comunican a través de contratos claros, eventos de dominio o servicios de aplicación livianos.
