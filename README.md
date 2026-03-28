# Lothal Basket Microservice Ecosystem

This project demonstrates a comprehensive, fully functional **.NET 8** microservice architecture. It showcases advanced distributed system patterns including **Clean Architecture**, **CQRS (Lothal.Mediator)**, **Outbox/Inbox Patterns**, **Event-Driven Architecture (NATS)**, an **API Gateway (YARP)** with built-in dynamic load balancing and rate limiting, and **Distributed Tracing** via OpenTelemetry.

Everything is containerized and orchestrated via **Docker Compose**, providing a seamless local development and deployment experience.

## 🚀 Features

*   **Basket Service (Producer API)**: A .NET 8 Minimal API handling basket write operations (Create, Get) using **PostgreSQL** (Entity Framework Core).
*   **Product Service (Data API)**: A .NET 8 Minimal API handling product transactions (Bulk merge, Get) using **Elasticsearch**.
*   **Stock Service (Data API)**: A robust .NET 8 microservice managing inventory tracking and synchronization across PostgreSQL and Redis. Integrates directly into NATS for immediate propagation.
*   **Admin Dashboard (UI)**: A rich, glassmorphic Vue 3 + Vite frontend for visualizing products, managing inventory, and orchestrating bulk warehouse operations.
*   **Clean Architecture & CQRS**: Business logic is completely decoupled using layers (`Api`, `Application`, `Domain`, `Infrastructure`) and the custom `Lothal.Mediator` package.
*   **Centralized Logging**: Seamlessly configured throughout all microservices using the shared `Lothal.BuildingBlocks` library, directing logs to **VictoriaLogs** via HTTP (Serilog + NDJSON batch formatter).
*   **Distributed Tracing (OpenTelemetry)**: End-to-end distributed traces collected from all services (Basket API, Product API, Consumer, API Gateway) via the shared `Lothal.BuildingBlocks` library and exported to **Jaeger** using OTLP. Traces include ASP.NET Core, HTTP client, NATS, Couchbase, and Npgsql spans.
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
├── Dockerfile.Stock            # Multi-stage Docker build for Stock Service
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
│   └── Product/                # Product Microservice (Product API)
│       ├── Lothal.Product.Api/             # Minimal API Endpoints
│       ├── Lothal.Product.Application/     # CQRS Handlers, Queries, Commands
│       ├── Lothal.Product.Domain/          # Entities
│       └── Lothal.Product.Infrastructure/  # Elasticsearch Integration
│   └── Stock/                  # Stock Microservice (Stock API)
│       ├── Lothal.Stock.Api/               # Endpoints for Inventory / Reservations
│       ├── Lothal.Stock.Application/       # Event handlers and Commands
│       ├── Lothal.Stock.Domain/            # Stock Entities
│       └── Lothal.Stock.Infrastructure/    # PostgreSQL & Redis Stock Repositories
│   └── UI/                     # Frontend Applications
│       └── lothal-admin-ui/                # Vue 3 + Vite Admin Dashboard
```

## 🐳 Running the Project (Docker Compose)

The easiest way to run the entire architecture (Gateway + Multiple Basket Replicas + Product API + Stock API + Consumer + NATS + PostgreSQL + Couchbase + Elasticsearch + VictoriaLogs + Grafana + Jaeger) is via Docker Compose.

1.  Open your terminal in the root directory (where `docker-compose.yml` is located).
2.  Build and start the containers in detached mode:

    ```bash
    docker compose up -d --build
    ```

> **Note:** The `docker-compose.yml` is configured to spin up **2 replicas** (`deploy: replicas: 2`) of the Basket API and Stock API automatically to demonstrate load balancing behind the API Gateway.

### 🖥️ Running the Admin UI

You can manage products and stocks using the included Vue dashboard natively on your host machine:
1. `cd src/UI/lothal-admin-ui`
2. `npm install`
3. `npm run dev`
4. Open your browser to `http://localhost:5173/`

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

### 3. Products App Endpoints

All product requests are routed via `/product-api/*`.

*   **Get Product:** `GET http://localhost:5024/product-api/api/products/{barcode}`
*   **Bulk Merge Products:** `POST http://localhost:5024/product-api/api/products/bulk-merge`
    ```json
    {
      "products": [
        { "barcode": "P2001", "price": 15.00, "name": "Hat", "class": "Accessories", "color": "Black", "size": "L" }
      ]
    }
    ```

### 4. Stock App Endpoints

All stock adjustments and queries are routed via `/api/stocks/*`.

*   **Get Stock:** `GET http://localhost:5024/api/stocks/{barcode}`
*   **Bulk Increase All:** `POST http://localhost:5024/api/stocks/bulk-increase`
    ```json
    {
      "amount": 1000,
      "transactionId": "b47...uuid"
    }
    ```

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
| Jaeger | `http://localhost:16686` |

## 🚀 Performance Testing (k6)

We use **k6** to validate the system under load. The test script simulates a full user journey: Create Basket → Add Items → Get Basket → Checkout.

### Running via Docker Compose (Recommended)
This runs k6 within the same Docker network as the services, ensuring perfect connectivity:
```bash
docker-compose --profile test up load-test
```

### Running Locally
If you have k6 installed:
```bash
k6 run tests/k6/load-test.js
```

### Running via Docker Standalone
```bash
docker run --rm -i grafana/k6 run - <tests/k6/load-test.js
```

### Thresholds
- **Success Rate**: > 99%
- **Latency (p95)**: < 500ms

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
*   **Vue 3, Vite & Pinia** (Admin Dashboard)
