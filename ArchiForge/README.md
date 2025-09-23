# 🚀 ArchiForge – API Generator

ArchiForge is a **.NET CLI tool** that automatically generates a **Clean Architecture / DDD skeleton in .NET 8**.  
It includes all the layers (Domain, Application, Infrastructure, API) and a working example with **Postgres + EF Core + Swagger**.

---

## 📦 Installation

```powershell
dotnet tool install --global ArchiForge
```

## 📂 Structure du projet

```

forged/
└─ MyForgedApi/
   ├─ MyForgedApi.sln
   ├─ MyForgedApi.Domain/         # Entities, ValueObjects, Domain Services, Events
   ├─ MyForgedApi.Application/    # CQRS (Commands, Queries, Handlers), DTOs, Interfaces
   ├─ MyForgedApi.Infrastructure/ # DbContext, Repositories, Impl. Services, Outbox, EventBus
   ├─ MyForgedApi.Api/            # Controllers, Middlewares, Configurations, Filters
   └─ docker-compose.yml    # Postgres + pgAdmin

```


---

## ⚡ Features

✅ Clean Architecture / DDD structure (Domain, Application, Infrastructure, API)

✅ CQRS-ready with MediatR + AutoMapper

✅ EF Core (Postgres/SQL Server/…) with a configured DbContext

✅ Example Todo Entity + Repository + CRUD Controller

✅ Swagger enabled for API documentation

✅ Global error-handling middleware

✅ Docker Compose (Postgres + pgAdmin preconfigured)

---

## 🔧 Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- PowerShell 7+ (Windows ou cross-platform)

---

## 🚀 Usage

```powershell
archiforge new MyForgedApi
```