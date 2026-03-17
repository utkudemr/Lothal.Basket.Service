# Lothal Basket Microservice Ecosystem

This project demonstrates a comprehensive, fully functional **.NET 8** microservice architecture. It showcases advanced distributed system patterns including **Clean Architecture**, **CQRS (Lothal.Mediator)**, **Outbox/Inbox Patterns**, **Event-Driven Architecture (NATS)**, an **API Gateway (YARP)** with built-in dynamic load balancing and rate limiting, and **Distributed Tracing** via OpenTelemetry.

Everything is containerized and orchestrated via **Docker Compose**, providing a seamless local development and deployment experience.

## 🚀 Features

*   **Basket Service (Producer API)**: A .NET 8 Minimal API handling basket write operations (Create, Get) using **PostgreSQL** (Entity Framework Core).
*   **Clean Architecture & CQRS**: Business logic is completely decoupled using layers (`Api`, `Application`, `Domain`, `Infrastructure`) and the custom `Lothal.Mediator` package.
*   **Centralized Logging**: Seamlessly configured throughout all microservices using the shared `Lothal.BuildingBlocks` library, directing logs to **VictoriaLogs** via HTTP (Serilog + NDJSON batch formatter).
*   **Distributed Tracing (OpenTelemetry)**: End-to-end distributed traces collected from all services (Basket API, Consumer, API Gateway) via the shared `Lothal.BuildingBlocks` library and exported to **Jaeger** using OTLP. Traces include ASP.NET Core, HTTP client, NATS, Couchbase, and Npgsql spans.
*   **Outbox Pattern**: The Basket API reliably saves domain events to an Outbox table in PostgreSQL within the same transaction as the business entity changes. A background worker then publishes these events to NATS, guaranteeing at-least-once delivery.
*   **NATS Messaging**: A lightweight, high-performance messaging system used as the event bus to decouple the producer and consumer.
*   **Basket Consumer (Worker Service)**: A separate .NET 8 worker service that listens to events from NATS.
*   **Inbox Pattern with Couchbase**: The Consumer uses **Couchbase** (NoSQL) to store incoming events (Inbox pattern) to guarantee idempotency and handles the read-model updates.
*   **YARP API Gateway**: A dedicated entry point routing all external HTTP requests to the underlying basket services.
*   **Docker DNS Load Balancing**: Dynamic Round-Robin load balancing across multiple microservice replicas using YARP and Docker's embedded DNS.
*   **Rate Limiting**: Native ASP.NET Core rate limiting integrated into the YARP API Gateway to protect backend services.

## 📁 Project Structure

```text
Lothal.Basket.Service/
├── docker-compose.yml          # Container orchestration (API, Consumer, Gateway, NATS, DBs, Observability)
├── Dockerfile                  # Multi-stage Docker build for Basket Service (Producer)
├── Dockerfile.ApiGateway       # Multi-stage Docker build for API Gateway (YARP)
├── Dockerfile.Consumer         # Multi-stage Docker build for Basket Consumer
├── src/
│   ├── Api/                    # Producer Microservice (Basket API)
│   │   ├── Lothal.Basket.Api/             # Minimal API Endpoints & Outbox Publisher Background Job
│   │   ├── Lothal.Basket.Application/     # CQRS Handlers, Queries, Commands
│   │   ├── Lothal.Basket.Domain/          # Entities (Basket, BasketItem, OutboxMessage)
│   │   └── Lothal.Basket.Infrastructure/  # EF Core AppDbContext, Repositories
│   ├── ApiGateway/             # YARP API Gateway Project
│   ├── BuildingBlocks/         # Shared Libraries (Logging & Telemetry Configs)
│   │   └── Lothal.BuildingBlocks/
│   │       ├── Logging/        # AddCustomLogging() — Serilog → VictoriaLogs
│   │       └── Telemetry/      # AddCustomTelemetry() — OpenTelemetry → Jaeger (OTLP)
│   └── Consumer/               # Consumer Microservice
│       └── Lothal.Basket.Consumer/        # NATS Listener & Couchbase Inbox Integration
```

## 🐳 Running the Project (Docker Compose)

The easiest way to run the entire architecture (Gateway + Multiple Basket Replicas + Consumer + NATS + PostgreSQL + Couchbase + VictoriaLogs + Grafana + Jaeger) is via Docker Compose.

1.  Open your terminal in the root directory (where `docker-compose.yml` is located).
2.  Build and start the containers in detached mode:

    ```bash
    docker compose up -d --build
    ```

> **Note:** The `docker-compose.yml` is configured to spin up **2 replicas** (`deploy: replicas: 2`) of the Basket API automatically to demonstrate load balancing behind the API Gateway.

## 🌐 API Endpoints & Testing

Once the containers are running, all requests should be routed through the **API Gateway** running on port `5024`.

### 1. Create a Basket (POST)
Creates a new basket for a customer using the CQRS Command pattern. This operation saves the basket and an `OutboxMessage` to PostgreSQL. The background job then publishes the event to NATS, which is finally consumed by the Consumer and saved into Couchbase.
*   **URL:** `http://localhost:5024/basket-api/api/baskets`
*   **Method:** `POST`
*   **Body:**
    ```json
    {
      "customerId": "user-123"
    }
    ```

### 2. Get a Basket (GET)
Retrieves a basket. This endpoint is extremely useful for demonstrating the **Round-Robin Load Balancing**.
*   **URL:** `http://localhost:5024/basket-api/api/baskets/{id}`
*   **Method:** `GET`

*(Replace `{id}` with the Guid returned from the POST request).*

### ⚖️ Testing Load Balancing

When you execute multiple `GET` or `POST` requests rapidly, inspect the response headers or logs. YARP smoothly distributes your requests across the available replicas (`basket-api-1` and `basket-api-2`), proving that the Docker network load balancing is actively working!

## 🛡️ Rate Limiting (YARP)

To protect backend services from being overwhelmed, the YARP API Gateway implements ASP.NET Core Native Rate Limiting using the **Fixed Window** algorithm. Depending on the endpoint, different policies apply:

*   **`create-basket-policy`**: Applied only to `POST /basket-api/api/baskets`. Highly restrictive (**5 requests / 10s**) to prevent spamming the database with write operations.
*   **`get-basket-policy`**: Applied to all other endpoints (like `GET /basket-api/api/baskets/{id}`). More relaxed (**20 requests / 10s**) as these are lightweight read operations.

If a limit is exceeded, the API Gateway immediately returns a **429 Too Many Requests** HTTP status code.

## 🔭 Distributed Tracing (Jaeger)

All services instrument outgoing and incoming requests using **OpenTelemetry** and export traces to **Jaeger** via the `OTEL_EXPORTER_OTLP_ENDPOINT` environment variable (`http://jaeger:4317` inside Docker).

The shared `AddCustomTelemetry(applicationName)` extension from `Lothal.BuildingBlocks` configures:
- **ASP.NET Core** instrumentation (incoming HTTP requests)
- **HTTP Client** instrumentation (outgoing HTTP requests)
- **NATS.Net**, **Couchbase**, **Npgsql**, and **YARP** activity sources

Once the containers are running, open the **Jaeger UI** to explore traces:

*   **URL:** `http://localhost:16686`

## 📊 Centralized Logging (VictoriaLogs + Grafana)

Logs from all services are shipped via HTTP using **Serilog** (NDJSON format) to **VictoriaLogs**.

| Service | URL |
|---|---|
| VictoriaLogs (query UI) | `http://localhost:9428` |
| Grafana | `http://localhost:3000` (admin / admin) |

## 🛠 Stopping the Project

To stop and clean up all containers and networks:

```bash
docker compose down
```

To entirely wipe volumes (PostgreSQL and Couchbase data):

```bash
docker compose down -v
```

## 📝 Technologies Used
*   **C# 12 / .NET 8**
*   **Lothal.Mediator** (Custom CQRS Dispatcher)
*   **NATS** (Event Bus / Messaging)
*   **Entity Framework Core & PostgreSQL** (Write Database & Outbox)
*   **Couchbase** (Read Database / Inbox)
*   **YARP** (Reverse Proxy & Rate Limiter)
*   **Serilog & VictoriaLogs** (Centralized Logging)
*   **OpenTelemetry & Jaeger** (Distributed Tracing)
*   **Docker / Docker Compose**
