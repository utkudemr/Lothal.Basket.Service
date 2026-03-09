# Lothal Basket Microservice & API Gateway

This project demonstrates a fully functional **.NET 8** microservice architecture utilizing **Minimal APIs**, **Clean Architecture**, **CQRS (MediatR)**, and **YARP (Yet Another Reverse Proxy)** for API Gateway integration.

Everything is containerized and orchestrated via **Docker Compose**, with built-in dynamic **Round-Robin Load Balancing** across service replicas.

## 🚀 Features

*   **Basket Service (Microservice)**: A .NET 8 Minimal API handling basket operations (Create, Get).
*   **Clean Architecture**: Separated into `Api`, `Application`, `Domain`, and `Infrastructure` layers.
*   **CQRS Pattern**: Business logic decoupled using the `MediatR` package.
*   **PostgreSQL**: Entity Framework Core integration with a Postgres database.
*   **YARP API Gateway**: A dedicated entry point routing requests to the underlying basket services.
*   **Docker DNS Load Balancing**: Dynamic Round-Robin load balancing across multiple microservice replicas using YARP and Docker's embedded DNS.

## 📁 Project Structure

```text
Lothal.Basket.Service/
├── docker-compose.yml          # Container orchestration (Gateway, DB, Replicas)
├── Dockerfile                  # Multi-stage Docker build for Basket Service
├── Dockerfile.ApiGateway       # Multi-stage Docker build for API Gateway
├── src/
│   ├── ApiGateway/             # YARP API Gateway Project
│   ├── Api/                    # Minimal API Endpoints (Presentation)
│   ├── Application/            # CQRS Handlers, Queries, Commands
│   ├── Domain/                 # Entities (Basket, BasketItem), Repository Interfaces
│   └── Infrastructure/         # EF Core AppDbContext, Repository Implementations
```

## 🐳 Running the Project (Docker Compose)

The easiest way to run the entire architecture (Gateway + Multiple Basket Replicas + Database) is via Docker Compose.

1.  Open your terminal in the root directory (where `docker-compose.yml` is located).
2.  Build and start the containers in detached mode:

    ```bash
    docker compose up -d --build
    ```

> **Note:** The `docker-compose.yml` is configured to spin up **2 replicas** (`deploy: replicas: 2`) of the Basket Service automatically.

## 🌐 API Endpoints & Testing

Once the containers are running, all requests should be routed through the **API Gateway** running on port `5024`.

### 1. Create a Basket (POST)
Creates a new basket for a customer.
*   **URL:** `http://localhost:5024/basket-api/api/baskets`
*   **Method:** `POST`
*   **Body:**
    ```json
    {
      "customerId": "user-123"
    }
    ```

### 2. Get a Basket (GET)
Retrieves a basket by its ID. This endpoint is extremely useful for demonstrating the **Round-Robin Load Balancing**.
*   **URL:** `http://localhost:5024/basket-api/api/baskets/{id}`
*   **Method:** `GET`

*(Replace `{id}` with the Guid returned from the POST request).*

### ⚖️ Testing Load Balancing

When you execute multiple `GET` or `POST` requests rapidly, inspect the response body. You will notice a `servedBy` property indicating the Docker Container ID (e.g., `ebc123...` vs `ab83...`).

YARP smoothly distributes your requests across the available replicas, proving that the load balancing is actively working!

## 🛡️ Rate Limiting (YARP)

To protect backend services from being overwhelmed, the YARP API Gateway implements ASP.NET Core Native Rate Limiting using the **Fixed Window** algorithm. Depending on the endpoint, different policies apply:

*   **`create-basket-policy`**: Applied only to `POST /basket-api/api/baskets`. Highly restrictive (**5 requests / 10s**) to prevent spamming the database with write operations.
*   **`get-basket-policy`**: Applied to all other endpoints (like `GET /basket-api/api/baskets/{id}`). More relaxed (**20 requests / 10s**) as these are lightweight read operations.

If a limit is exceeded, the API Gateway immediately returns a **429 Too Many Requests** HTTP status code without forwarding the request.

## 🛠 Stopping the Project

To stop and clean up all containers and networks:

```bash
docker compose down
```

## 📝 Technologies Used
*   **C# 12 / .NET 8**
*   **YARP** (Reverse Proxy & Rate Limiter)
*   **MediatR** (CQRS)
*   **Entity Framework Core**
*   **PostgreSQL**
*   **Docker / Docker Compose**
