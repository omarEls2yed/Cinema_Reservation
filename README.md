# 🎬 High-Concurrency-Reservation-Engine Api

A robust, enterprise-grade reservation backend built with **ASP.NET Core (.NET 8)**. This system is designed to handle high-concurrency ticket bookings for cinema events while ensuring zero data loss and preventing "double-booking" through a distributed architecture.

---

## 🏗️ Architecture Overview

The project follows **Clean Architecture** principles, separating concerns into Domain, Application, Infrastructure, and Web API layers. This ensures the system is decoupled, testable, and maintainable.



### 🛠️ Key Architectural Decisions

* **Distributed Locking (Redis)**: Implemented a multi-layered locking strategy (Standard Lock + Processing Flag) to solve the "double-click" race condition and ensure absolute seat ownership during the payment window.
* **Asynchronous Messaging (RabbitMQ & MassTransit)**: Utilizes an **Asynchronous Request-Reply** pattern. The API remains responsive by offloading heavy business logic (payment, ticket generation) to a background **Booking Consumer**.
* **Unique Constraints**: Enforced data integrity at the database level using composite unique indexes in Entity Framework Core, ensuring a specific seat (Row + Number) is unique per Venue.
* **Specification Pattern**: Abstracted query logic into reusable specifications, keeping the services thin and the database queries highly optimized.

---


## 🚀 Technical Stack

* **Framework**: .NET 8 Web API
* **Database**: SQL Server (EF Core)
* **Messaging**: RabbitMQ / MassTransit
* **Caching & Locking**: Redis (StackExchange.Redis)
* **Design Patterns**: Clean Architecture, Repository, Unit of Work, Specification, Saga Pattern (Basic).

---

## 📖 How It Works (The Booking Flow)



1.  **Lock**: User selects a seat; a Redis lock is established for 10 minutes to hold the selection.
2.  **Book**: User initiates payment. The API sets an atomic `processing` flag in Redis and publishes a `BookingMessage` to the queue.
3.  **Process**: The `BookingConsumer` picks up the message, performs idempotency checks, processes the payment, and persists the `Ticket` to SQL Server.
4.  **Cleanup**: Upon completion, the consumer releases both the seat lock and the processing flag, finalizing the seat's status.

---

## 🔧 Setup & Installation

1.  **Clone the Repository**:
    ```bash
    git clone [https://github.com/omarEls2yed/High-Concurrency-Reservation-Engine.git](https://github.com/omarEls2yed/High-Concurrency-Reservation-Engine.git)
    ```
2.  **Configuration**: Update `appsettings.json` with your specific connection strings for:
    * SQL Server
    * Redis
    * RabbitMQ
3.  **Database Migration**:
    ```bash
    dotnet ef database update
    ```
4.  **Testing**: Use the provided Postman collection to test the end-to-end flow:
    `Login -> Create Event -> Create Seat Row -> Lock Seat -> Book Ticket`.

