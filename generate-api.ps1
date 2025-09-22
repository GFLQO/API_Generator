param(
    [Parameter(Mandatory=$true)]
    [string]$Name
)

# Création du dossier racine
New-Item -ItemType Directory -Force -Path $Name | Out-Null
Set-Location $Name

# Solution
dotnet new sln -n $Name

# Domain
dotnet new classlib -n "$Name.Domain"
dotnet sln add "$Name.Domain\$Name.Domain.csproj"

# Application
dotnet new classlib -n "$Name.Application"
dotnet add "$Name.Application\$Name.Application.csproj" reference "$Name.Domain\$Name.Domain.csproj"
dotnet sln add "$Name.Application\$Name.Application.csproj"

# Infrastructure
dotnet new classlib -n "$Name.Infrastructure"
dotnet add "$Name.Infrastructure\$Name.Infrastructure.csproj" reference "$Name.Domain\$Name.Domain.csproj"
dotnet add "$Name.Infrastructure\$Name.Infrastructure.csproj" reference "$Name.Application\$Name.Application.csproj"
dotnet sln add "$Name.Infrastructure\$Name.Infrastructure.csproj"

# API
dotnet new webapi -n "$Name.Api"
dotnet add "$Name.Api\$Name.Api.csproj" reference "$Name.Application\$Name.Application.csproj"
dotnet add "$Name.Api\$Name.Api.csproj" reference "$Name.Infrastructure\$Name.Infrastructure.csproj"
dotnet sln add "$Name.Api\$Name.Api.csproj"

# Packages
dotnet add "$Name.Application" package MediatR
dotnet add "$Name.Application" package AutoMapper
dotnet add "$Name.Infrastructure" package Microsoft.EntityFrameworkCore
dotnet add "$Name.Infrastructure" package Microsoft.EntityFrameworkCore.Design
dotnet add "$Name.Infrastructure" package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add "$Name.Api" package Swashbuckle.AspNetCore

# Dossiers
$folders = @(
    "$Name.Domain\Entities","$Name.Domain\ValueObjects","$Name.Domain\Events","$Name.Domain\Services","$Name.Domain\Exceptions","$Name.Domain\Specifications",
    "$Name.Application\Interfaces","$Name.Application\Services","$Name.Application\DTOs","$Name.Application\Commands","$Name.Application\Queries","$Name.Application\Handlers","$Name.Application\Validators","$Name.Application\Mapping","$Name.Application\UseCases",
    "$Name.Infrastructure\Persistence","$Name.Infrastructure\Repositories","$Name.Infrastructure\Services","$Name.Infrastructure\Migrations","$Name.Infrastructure\Outbox","$Name.Infrastructure\EventBus","$Name.Infrastructure\Technical",
    "$Name.Api\Controllers","$Name.Api\Middlewares","$Name.Api\Configurations","$Name.Api\Filters","$Name.Api\Dtos"
)
foreach ($folder in $folders) { New-Item -ItemType Directory -Force -Path $folder | Out-Null }

# Exemple Entity
$entityCode = @"
namespace $Name.Domain.Entities
{
    public class Todo
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsDone { get; set; }
    }
}
"@
Set-Content "$Name.Domain\Entities\Todo.cs" $entityCode

# DbContext
$dbContextCode = @"
using Microsoft.EntityFrameworkCore;
using $Name.Domain.Entities;

namespace $Name.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Todo> Todos => Set<Todo>();
    }
}
"@
Set-Content "$Name.Infrastructure\Persistence\AppDbContext.cs" $dbContextCode

# Repository
$repoCode = @"
using $Name.Domain.Entities;
using $Name.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace $Name.Infrastructure.Repositories
{
    public class TodoRepository
    {
        private readonly AppDbContext _db;
        public TodoRepository(AppDbContext db) => _db = db;

        public async Task<List<Todo>> GetAllAsync() => await _db.Todos.ToListAsync();

        public async Task<Todo> AddAsync(Todo todo)
        {
            _db.Todos.Add(todo);
            await _db.SaveChangesAsync();
            return todo;
        }
    }
}
"@
Set-Content "$Name.Infrastructure\Repositories\TodoRepository.cs" $repoCode

# Middleware Global
$middlewareCode = @"
using System.Net;
using System.Text.Json;

namespace $Name.Api.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                var result = JsonSerializer.Serialize(new { error = ex.Message });
                await context.Response.WriteAsync(result);
            }
        }
    }

    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder) 
            => builder.UseMiddleware<ErrorHandlingMiddleware>();
    }
}
"@
Set-Content "$Name.Api\Middlewares\ErrorHandlingMiddleware.cs" $middlewareCode

# Controller
$controllerCode = @"
using Microsoft.AspNetCore.Mvc;
using $Name.Domain.Entities;
using $Name.Infrastructure.Repositories;

namespace $Name.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodoController : ControllerBase
    {
        private readonly TodoRepository _repo;
        public TodoController(TodoRepository repo) => _repo = repo;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _repo.GetAllAsync());

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Todo todo) => Ok(await _repo.AddAsync(todo));
    }
}
"@
Set-Content "$Name.Api\Controllers\TodoController.cs" $controllerCode

# Program.cs
$programCode = @"
using $Name.Infrastructure.Persistence;
using $Name.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using $Name.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<TodoRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseErrorHandling();
app.MapControllers();
app.Run();
"@
Set-Content "$Name.Api\Program.cs" $programCode

# appsettings.json
$appSettings = @"
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=${Name.ToLower()}_db;Username=postgres;Password=postgres"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
"@
Set-Content "$Name.Api\appsettings.json" $appSettings

# Docker Compose
$dockerCompose = @"
version: '3.9'
services:
  db:
    image: postgres:16
    container_name: ${Name.ToLower()}_db
    restart: always
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: ${Name.ToLower()}_db
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data

  pgadmin:
    image: dpage/pgadmin4
    container_name: ${Name.ToLower()}_pgadmin
    restart: always
    environment:
      PGADMIN_DEFAULT_EMAIL: admin@admin.com
      PGADMIN_DEFAULT_PASSWORD: admin
    ports:
      - "5050:80"
    depends_on:
      - db

volumes:
  pgdata:
"@
Set-Content "docker-compose.yml" $dockerCompose

Write-Host "✅ Projet $Name généré avec succès !"
Write-Host "👉 docker-compose up -d (DB + pgAdmin)"
Write-Host "👉 cd $Name.Api ; dotnet run"
Write-Host "Swagger: https://localhost:5001/swagger"
Write-Host "API: GET/POST https://localhost:5001/api/todo"
