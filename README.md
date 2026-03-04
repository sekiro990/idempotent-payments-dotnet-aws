
# Project Title

# Idempotent Payments Processing API


## Features

- Idempotent payment creation
- Redis fast-path replay detection
- PostgreSQL unique constraint protection
- Race-condition-safe under concurrent load
- Clean Architecture structure
- Docker-based local development
- Distributed-system ready design


## Tech Stack

- .NET 9
- ASP.NET Core Web API
- Entity Framework Core 9
- PostgreSQL (Docker)
- Redis (Docker)
- Npgsql
- Dependency Injection
- Clean Architecture
## Installation

## Prerequisites

- .NET 9 SDK
- Docker Desktop
- Git

## Clone the repository

git clone https://github.com/your-username/idempotent-payments-dotnet-aws.git
cd idempotent-payments-dotnet-aws
## Run Locally
## 1️⃣ Start Postgres & Redis

docker compose up -d

## 2️⃣ Apply Migrations

dotnet ef database update --project src/Payments.Infrastructure --startup-project src/Payments.Api

## 3️⃣ Run API

dotnet run --project src/Payments.Api --launch-profile https

API will run at:
https://localhost:7244

## API Reference

---

### Create Payment

```http
POST /api/payments
```

| Parameter         | Type     | Description                                      |
| :---------------- | :------- | :----------------------------------------------- |
| `Idempotency-Key` | `string` | **Required**. Unique key to prevent duplicates   |

---

### Request Body

```json
{
  "userId": "user-1",
  "amount": 150.75,
  "currency": "AUD"
}
```

| Parameter  | Type      | Description                                   |
| :----------| :-------- | :-------------------------------------------- |
| `userId`   | `string`  | **Required**. Identifier of the user          |
| `amount`   | `decimal` | **Required**. Payment amount                  |
| `currency` | `string`  | **Required**. ISO currency code (AUD, USD)    |

---

### First Response

Status: `201 Created`

| Field      | Type     | Description                                |
| :----------| :------- | :----------------------------------------- |
| `isReplay` | `bool`   | Indicates whether request was replayed     |
| `payment`  | `object` | The created payment details                |

---

### Replay Response

Status: `200 OK`

| Field      | Type     | Description                                      |
| :----------| :------- | :----------------------------------------------- |
| `isReplay` | `bool`   | Always `true` for duplicate idempotent calls    |
| `payment`  | `object` | Previously created payment                      |

---

### Error Response

Status: `400 Bad Request`

| Field   | Type     | Description                                  |
| :------ | :------- | :------------------------------------------- |
| `error` | `string` | Error message describing the issue           |
## Optimizations

- Redis TTL reduces database reads
- Unique DB index ensures hard safety
- Exception-based conflict resolution avoids extra locking
## FAQ

#### Why not rely only on Redis?

Redis is not the source of truth. Database uniqueness guarantees correctness.


#### Why catch Postgres error 23505?

It indicates duplicate composite key insert — used to safely handle race conditions.


## License

[MIT](https://choosealicense.com/licenses/mit/)


## 🚀 About Me
Backend-focused Software Engineering student at RMIT University.

Interested in:
- Distributed systems
- Fintech architecture
- Cloud engineering
- Reliable system design


## Acknowledgements

Inspired by real-world payment system design patterns used in fintech platforms.

## 🔗 Links
[![portfolio](https://img.shields.io/badge/my_portfolio-000?style=for-the-badge&logo=ko-fi&logoColor=white)](https://meetarnav.netlify.app)
[![linkedin](https://img.shields.io/badge/linkedin-0A66C2?style=for-the-badge&logo=linkedin&logoColor=white)](https://www.linkedin.com/in/arnav-jain-9722a72b7/$0)


