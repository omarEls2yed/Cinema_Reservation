# 🎬 High-Concurrency Reservation Engine

A production-grade, distributed ticket reservation backend built with **ASP.NET Core (.NET 8)**. Engineered to handle thousands of simultaneous booking requests without double-selling a single seat.

---

## 📌 What Problem Does This Solve?

Imagine a concert or cinema event goes on sale and thousands of users try to book the same seat at the exact same second. A naive system would fail — it would oversell seats, corrupt data, or crash under load.

This system solves that with a **multi-layered concurrency strategy**: distributed Redis locks prevent race conditions, an asynchronous message queue decouples the booking pipeline from the HTTP request, a transactional outbox guarantees zero message loss, and a database-level unique index acts as the final safety net. The result is a system that remains fast and responsive regardless of load, while making double-booking structurally impossible.

---

## 🏗️ Architecture

The project follows **Clean Architecture** principles, organized into four layers with strict one-directional dependencies.

```
Cinema-Reservation/          → Web API Entry Point (Program.cs, Middleware)
├── Core/
│   ├── DomainLayer/         → Entities, Contracts, Custom Exceptions
│   ├── ServiceAbstraction/  → Service Interfaces
│   └── Service/             → Business Logic, Consumers, Specifications
├── Infrastructure/
│   ├── Persistence/         → EF Core DbContext, Repositories, Migrations
│   └── Presentation/        → API Controllers
└── Shared/                  → DTOs, Messages, Hubs, Shared Models
```

**Key patterns used:** Repository, Unit of Work, Specification, Service Manager, Async Request-Reply.

---

## ⚙️ Technical Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core (.NET 8) Web API |
| Database | SQL Server via Entity Framework Core 8 |
| Distributed Cache & Locking | Redis (StackExchange.Redis) |
| Message Broker | RabbitMQ via MassTransit 8 |
| Reliable Messaging | MassTransit Transactional Outbox |
| Authentication | Keycloak (JWT / OpenID Connect) |
| Real-Time Notifications | SignalR |
| PDF Generation | QuestPDF |
| Email | MailKit (SMTP) |
| Object Mapping | AutoMapper |

---

## 🔐 Authentication

Authentication is handled by **Keycloak**, an enterprise-grade identity provider. The API validates JWT Bearer tokens on every protected endpoint. Role-based authorization (`Admin` role) protects all admin operations such as creating events and managing seats.

---

## 🚀 The Booking Flow (How It Works)

This is the core of the system. Every step is designed to prevent a race condition.

```
User                    API                     Redis                  RabbitMQ              Consumer               Database
 |                       |                        |                       |                      |                      |
 |-- POST /lock -------> |                        |                       |                      |                      |
 |                       |-- SET lock (NX, 10m)-> |                       |                      |                      |
 |                       |<-- OK (lock acquired) -|                       |                      |                      |
 |<-- 200 Seat Locked -- |                        |                       |                      |                      |
 |                       |                        |                       |                      |                      |
 |-- POST /book -------> |                        |                       |                      |                      |
 |                       |-- SET processing (NX)->|                       |                      |                      |
 |                       |-- Verify lock owner -->|                       |                      |                      |
 |                       |-- Publish BookingMsg ->|                       |-> Consumer picks up ->|                      |
 |<-- 200 + TrackingId - |                        |                       |                      |-- Idempotency check ->|
 |                       |                        |                       |                      |-- Process Payment     |
 |                       |                        |                       |                      |-- Save Ticket ------> |
 |                       |                        |                       |                      |-- Delete Redis keys ->|
 |                       |                        |                       |<- Publish Completed --                       |
 |<-- SignalR Result ---- |                        |                       |                      |                      |
```

**Step-by-step:**

1. **Lock** — The user selects a seat. The API sets a Redis key (`lock:session:{eventId}:seat:{seatId}`) using `SET NX` (only succeeds if no key exists), giving the user exclusive ownership for 10 minutes.

2. **Book** — The user proceeds to pay. The API sets an atomic `processing` flag in Redis to block any duplicate click, verifies the user still owns the lock, then publishes a `BookingMessage` to RabbitMQ and immediately returns a tracking ID to the user. The API never blocks waiting for payment.

3. **Process** — The `BookingConsumer` picks up the message from the queue. It performs an idempotency check (ensuring this exact booking hasn't already been processed), calls the payment service, and on success, persists the `Ticket` to SQL Server. The entire publish + save is wrapped in a **MassTransit Transactional Outbox**, meaning the message will not be lost even if the broker is temporarily unavailable.

4. **Notify** — On success or failure, the consumer publishes a `BookingCompletedEvent`. The `BookingNotificationConsumer` receives this and pushes the result in real-time to the user's browser via **SignalR**.

5. **Cleanup** — In a `finally` block, both the seat lock and the processing flag are always deleted from Redis, regardless of outcome.

---

## 🛡️ Concurrency & Data Integrity Guarantees

| Layer | Mechanism | Protects Against |
|---|---|---|
| Redis Lock (`SET NX`) | Distributed lock per seat per event | Multiple users selecting the same seat simultaneously |
| Redis Processing Flag | Atomic flag during booking | Same user double-clicking "Book" |
| Idempotency Key in Consumer | Redis `SET NX` on `BookingId` | Duplicate messages being replayed by the broker |
| DB Unique Index | `(EventId, SeatId)` composite index on `Tickets` | Any edge case that bypasses all the above |
| Transactional Outbox | MassTransit EF Core Outbox | Messages being lost if RabbitMQ is briefly down |

---

## 📡 API Endpoints

### Events (`/api/Events`)
| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/futureEvents` | Public | Get paginated list of upcoming events |
| POST | `/create` | Admin | Create a new event |
| PUT | `/update` | Admin | Update an existing event |
| DELETE | `/delete/{id}` | Admin | Delete an event |

### Seats (`/api/Seat`)
| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/Get-all-seats-for-event` | Public | Get all seats for an event (includes live Redis status) |
| GET | `/{id}` | Public | Get a single seat by ID (cached 60s) |
| POST | `/Create-row-of-seats` | Admin | Create a new row of seats for a venue |
| DELETE | `/Delete-seat-from-Venue` | Admin | Delete a specific seat |

### Reservation (`/api/Reservation`)
| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/lock` | User | Lock a seat for 10 minutes |
| POST | `/unlock` | User | Release a seat lock early |
| POST | `/book` | User | Initiate booking — returns tracking ID immediately |

### Tickets (`/api/Tickets`)
| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/{id}` | Public | Get ticket details by ID |
| GET | `/{id}/print` | Public | Download ticket as a PDF |

---

## 🔧 Setup & Installation

### Prerequisites

Make sure you have the following running before starting:
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- SQL Server (local or Docker)
- Redis (local or Docker)
- RabbitMQ (local or Docker)
- Keycloak (local or Docker)

**Quick start with Docker:**
```bash
docker run -d -p 6379:6379 redis
docker run -d -p 5672:5672 -p 15672:15672 rabbitmq:management
docker run -d -p 8080:8080 -e KEYCLOAK_ADMIN=admin -e KEYCLOAK_ADMIN_PASSWORD=admin quay.io/keycloak/keycloak:latest start-dev
```

### 1. Clone the Repository

```bash
git clone https://github.com/omarEls2yed/High-Concurrency-Reservation-Engine.git
cd High-Concurrency-Reservation-Engine
```

### 2. Configure `appsettings.json`

Open `Cinema-Reservation/appsettings.json` and update the connection strings and credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=ReservationDb;Trusted_Connection=True;TrustServerCertificate=True;",
    "Redis": "localhost:6379"
  },
  "RabbitMQ": {
    "ConnectionString": "rabbitmq://guest:guest@localhost"
  },
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/YOUR_REALM",
    "ClientId": "YOUR_CLIENT_ID",
    "Audience": "account",
    "Issuer": "http://localhost:8080/realms/YOUR_REALM",
    "MetadataAddress": "http://localhost:8080/realms/YOUR_REALM/.well-known/openid-configuration"
  },
  "EmailSettings": {
    "Host": "sandbox.smtp.mailtrap.io",
    "Port": 587,
    "SenderName": "Reservation Platform",
    "SenderEmail": "your-email@example.com",
    "UserName": "your-mailtrap-username",
    "Password": "your-mailtrap-password"
  }
}
```

### 3. Apply Database Migrations

```bash
cd Cinema-Reservation
dotnet ef database update
```

### 4. Run the Application

```bash
dotnet run
```

Swagger UI will be available at `http://localhost:5280/swagger`.

---

## 🧪 Testing the Full Flow

Use Postman or any API client. Follow these steps in order:

1. **Authenticate** — Obtain a JWT token from Keycloak for an Admin user.
2. **Create a Venue** — Seed the database with a venue directly or via a migration.
3. **Create an Event** — `POST /api/Events/create` with the venue ID and event details.
4. **Create Seats** — `POST /api/Seat/Create-row-of-seats` to add a row of seats to the venue.
5. **Lock a Seat** — `POST /api/Reservation/lock` with your user's JWT token.
6. **Book the Seat** — `POST /api/Reservation/book` — you'll receive a tracking ID.
7. **Get the Result** — Connect to the SignalR hub (`/bookingHub`) to receive the real-time booking result.
8. **Download Ticket** — `GET /api/Tickets/{id}/print` to get your PDF ticket.

---

## 📁 Project Structure Reference

```
Core/DomainLayer/Models/         → Event, Seat, Ticket, Venue, UserPaymentMethod
Core/DomainLayer/Contracts/      → IGenericRepository, ISpecification, IUnitOfWork
Core/DomainLayer/Exceptions/     → Domain-specific exceptions (SeatNotFound, etc.)
Core/Service/                    → EventService, SeatService, SeatReservationService, PaymentService
Core/Service/Consumers/          → BookingConsumer, BookingNotificationConsumer
Core/Service/Specifications/     → All query specifications (FutureEvent, SeatTicket, etc.)
Infrastructure/Persistence/      → EF Core DbContext, Generic Repository, UnitOfWork
Infrastructure/Presentation/     → API Controllers
Shared/                          → DTOs, message contracts, SignalR hub, pagination
```

---

## 👤 Author

**Omar Elsayed**
- GitHub: [@omarEls2yed](https://github.com/omarEls2yed)
