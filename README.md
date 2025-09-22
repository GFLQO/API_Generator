# 🚀 $NAME API Generator

Ce projet est un **squelette Clean Architecture / DDD en .NET 8**, généré automatiquement via un script PowerShell.  
Il inclut toutes les couches Domain, Application, Infrastructure, API, et un exemple fonctionnel avec **Postgres + EF Core + Swagger**.

---

## 📂 Structure du projet

```

$NAME/
├─ $NAME.sln
├─ $NAME.Domain/ # Entités, ValueObjects, Domain Services, Events
├─ $NAME.Application/ # CQRS (Commands, Queries, Handlers), DTOs, Interfaces
├─ $NAME.Infrastructure/# DbContext, Repositories, Impl. Services, Outbox, EventBus
├─ $NAME.Api/ # Controllers, Middlewares, Configurations, Filters
└─ docker-compose.yml # Postgres + pgAdmin

```


---

## ⚡ Fonctionnalités incluses

- ✅ **Architecture DDD / Clean** (Domain, Application, Infrastructure, API)  
- ✅ **CQRS-ready** avec MediatR + AutoMapper  
- ✅ **EF Core (Postgres)** + DbContext configuré  
- ✅ **Exemple Todo Entity + Repository + Controller CRUD**  
- ✅ **Swagger** activé pour la doc d’API  
- ✅ **Middleware global de gestion d’erreurs**  
- ✅ **Docker Compose** (Postgres + pgAdmin)  

---

## 🔧 Prérequis

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- PowerShell 7+ (Windows ou cross-platform)

---

## 🚀 Lancer le projet

```powershell
.\generate-api.ps1 MyApi
```