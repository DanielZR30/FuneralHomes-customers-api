# Módulo de Mensajería e Integraciones (`FH.Modules.Messaging`)

Este módulo forma parte de la arquitectura del microservicio **Customers** (`fh.api.customer`) y es el componente encargado de la **comunicación asíncrona orientada a eventos (*Event-Driven Architecture*)** mediante **Apache Kafka**.

---

## 1. Responsabilidades del Módulo

El módulo `FH.Modules.Messaging` es responsable de conectar el dominio de Customers con el resto del ecosistema de la plataforma Funeral Homes:

1. **Publicación Confiable de Eventos en Apache Kafka**:
   - Serializar y enviar eventos de integración a los tópicos correspondientes con semántica *at-least-once*.
   - Implementar particionamiento por clave (`subscriptionId` o `customerId`) para garantizar orden de eventos en la misma partición.
2. **Implementación del Outbox Worker / Dispatcher**:
   - Ejecutar un servicio en segundo plano (`BackgroundService`) que sondea periódicamente la tabla `beneficiary_audit_log` en busca de registros con `event_published = FALSE`.
   - Publicar el evento al broker y marcar `event_published = TRUE` de forma idempotente.
3. **Integración con Servicios Consumidores**:
   - **Financials (`fh.api.financial`)**: Notificar adiciones y retiros de beneficiarios para el recálculo dinámico de la cuota periódica según la edad derivada y tipo de miembro.
   - **Notifications (`fh.api.notification`)**: Notificar al cliente titular la confirmación de la inclusión o retiro del beneficiario.
4. **Resiliencia y Manejo de Errores**:
   - Reintentos exponenciales ante interrupciones de conectividad con Kafka.
   - Envío a tópicos de mensajes muertos (Dead Letter Queue / DLQ) o alertas tras agotar reintentos máximos.

---

## 2. Historias de Usuario (User Stories)

### HU-MSG-01: Publicación de Evento `customer.beneficiary.added` en Kafka
* **Como** Sistema de Facturación y Finanzas (`fh.api.financial`),  
* **Quiero** consumir el evento `customer.beneficiary.added` tan pronto se afilia un miembro a una póliza,  
* **Para** recalcular la prima o cuota mensual en función de la edad derivada y parentesco del nuevo beneficiario.

#### Criterios de Aceptación (Gherkin):
* **Escenario 1: Emisión de evento con payload estándar**
  - **Dado** que se ha registrado un beneficiario y se procesa su registro en el outbox,
  - **Cuando** el despachador de mensajería procesa el registro,
  - **Entonces** debe publicar en el tópico de Kafka un mensaje JSON con `eventType: BENEFICIARY_ADDED`, datos de suscripción, miembro, edad derivada, tipo de sujeto (`HUMAN`/`PET`) y parentesco.
* **Escenario 2: Mensaje con clave de partición**
  - **Dado** un evento para la suscripción con UUID X,
  - **Cuando** se publica a Kafka,
  - **Entonces** el mensaje debe usar como Message Key el `subscriptionId` para garantizar orden cronológico de eventos para esa póliza.

---

### HU-MSG-02: Publicación de Evento `customer.beneficiary.removed`
* **Como** Microservicio de Notificaciones (`fh.api.notification`),  
* **Quiero** recibir el evento de desafiliación de un beneficiario,  
* **Para** enviar un correo electrónico/SMS de confirmación al titular informando el retiro del miembro.

#### Criterios de Aceptación:
* **Escenario 1: Notificación de retiro emitida a Kafka**
  - **Dado** un beneficiario retirado en el módulo de beneficiarios,
  - **Cuando** el dispatcher lee la novedad,
  - **Entonces** se emite el evento `customer.beneficiary.removed` con `subscriptionId`, `memberId` y fecha de retiro en UTC.

---

### HU-MSG-03: Procesamiento en Segundo Plano de Eventos Pendientes (Outbox Worker)
* **Como** Plataforma Resiliente,  
* **Quiero** que un worker procese continuamente los eventos almacenados en el outbox de base de datos,  
* **Para** que la API web responda rápidamente a los usuarios sin verse ralentizada ni bloqueada por la latencia del broker de mensajería.

#### Criterios de Aceptación:
* **Escenario 1: Detección y publicación automática**
  - **Dado** 3 registros no publicados en la tabla de auditoría,
  - **Cuando** el `OutboxDispatcherService` ejecuta su ciclo de trabajo,
  - **Entonces** envía los 3 mensajes a Kafka, recibe la confirmación (ack) del broker y actualiza `event_published = TRUE` en base de datos.
* **Escenario 2: Base de datos sin eventos pendientes**
  - **Dado** que todos los eventos están publicados,
  - **Cuando** corre el worker,
  - **Entonces** duerme durante el intervalo configurado (e.g. 5 segundos) sin sobrecargar la base de datos.

---

### HU-MSG-04: Manejo de Caídas y Reconexión con el Broker
* **Como** Operador de Infraestructura,  
* **Quiero** que el servicio reintente automáticamente si el broker de Kafka está temporalmente inaccesible,  
* **Para** garantizar que ningún evento se pierda (*Zero Data Loss*).

#### Criterios de Aceptación:
* **Escenario 1: Broker no disponible**
  - **Dado** que el clúster de Kafka está caído,
  - **Cuando** el worker intenta publicar,
  - **Entonces** captura la excepción de Kafka, registra un log de error de severidad Warning/Error, no marca el evento como publicado y vuelve a intentar en el siguiente ciclo tras recuperar la conexión.

---

## 3. Explicación Técnica y Arquitectura

### 3.1 Estructura Interna del Módulo
Ubicación: `carpeta Messaging en Application / Infrastructure`

```text
FH.Modules.Messaging/
├── Domain/
│   └── Events/                  # Contratos de eventos de integración
│       ├── BeneficiaryAddedIntegrationEvent.cs
│       └── BeneficiaryRemovedIntegrationEvent.cs
│
├── Application/
│   ├── IEventPublisher.cs       # Abstracción para publicación de eventos
│   └── IOutboxDispatcher.cs     # Contrato para el despachador
│
├── Infrastructure/              # Adaptadores de Kafka
│   ├── Kafka/
│   │   ├── KafkaEventPublisher.cs   # Implementación con Confluent.Kafka
│   │   └── KafkaProducerFactory.cs  # Configuración de ProducerConfig
│   └── BackgroundServices/
│       └── OutboxDispatcherWorker.cs # BackgroundService de .NET
│
└── README.md                    # Este archivo
```

### 3.2 Contrato JSON del Evento Kafka

#### Tópico: `fh.customer.beneficiary-events`
#### Evento: `customer.beneficiary.added`
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

#### Evento: `customer.beneficiary.removed`
```json
{
  "eventId": "789e0123-e45b-67d8-a901-426614174888",
  "eventType": "BENEFICIARY_REMOVED",
  "timestamp": "2026-10-03T11:30:00Z",
  "data": {
    "subscriptionId": "987e6543-e21b-12d3-a456-426614174111",
    "memberId": "456e7890-e89b-12d3-a456-426614174222",
    "removedAt": "2026-10-03T11:30:00Z"
  }
}
```

### 3.3 Configuración de Productor Kafka (`appsettings.json`)
```json
{
  "Kafka": {
    "BootstrapServers": "localhost:9092",
    "BeneficiaryEventsTopic": "fh.customer.beneficiary-events",
    "Acks": "All",
    "EnableIdempotence": true,
    "MessageTimeoutMs": 5000,
    "RetryCount": 3
  }
}
```

### 3.4 Diagrama de Secuencia del Outbox Worker
```text
[Base de Datos]                   [Outbox Worker]                 [Apache Kafka]
      │                                  │                              │
      │── 1. SELECT WHERE published=F ──>│                              │
      │                                  │── 2. ProduceAsync(Topic) ───>│
      │                                  │<── 3. Ack (Offset / OK) ─────│
      │<── 4. UPDATE published = TRUE ───│                              │
      │                                  │                              │
```
