using ArchiForge.Utils;
using System.IO;

namespace ArchiForge.Generators
{
    public static class SolutionGenerator
    {
        public static void Generate(string root, string name, string db, bool cqrs, bool swagger, string ddd, string[] entities)
        {
            ShellRunner.Run($"dotnet new sln -n {name}", root);

            ShellRunner.Run($"dotnet new classlib -n {name}.Domain", root);
            ShellRunner.Run($"dotnet sln add {name}.Domain/{name}.Domain.csproj", root);

            ShellRunner.Run($"dotnet new classlib -n {name}.Application", root);
            ShellRunner.Run($"dotnet add {name}.Application/{name}.Application.csproj reference {name}.Domain/{name}.Domain.csproj", root);
            ShellRunner.Run($"dotnet sln add {name}.Application/{name}.Application.csproj", root);

            ShellRunner.Run($"dotnet new classlib -n {name}.Infrastructure", root);
            ShellRunner.Run($"dotnet add {name}.Infrastructure/{name}.Infrastructure.csproj reference {name}.Domain/{name}.Domain.csproj", root);
            ShellRunner.Run($"dotnet add {name}.Infrastructure/{name}.Infrastructure.csproj reference {name}.Application/{name}.Application.csproj", root);
            ShellRunner.Run($"dotnet sln add {name}.Infrastructure/{name}.Infrastructure.csproj", root);

            ShellRunner.Run($"dotnet new webapi -n {name}.Api", root);
            ShellRunner.Run($"dotnet add {name}.Api/{name}.Api.csproj reference {name}.Application/{name}.Application.csproj", root);
            ShellRunner.Run($"dotnet add {name}.Api/{name}.Api.csproj reference {name}.Infrastructure/{name}.Infrastructure.csproj", root);
            ShellRunner.Run($"dotnet sln add {name}.Api/{name}.Api.csproj", root);

            ShellRunner.Run($"dotnet new xunit -n {name}.Tests", root);
            ShellRunner.Run($"dotnet sln add {name}.Tests/{name}.Tests.csproj", root);
            ShellRunner.Run($"dotnet add {name}.Tests/{name}.Tests.csproj reference {name}.Application/{name}.Application.csproj", root);
            ShellRunner.Run($"dotnet add {name}.Tests/{name}.Tests.csproj package FluentAssertions", root);

            ShellRunner.Run($"dotnet add {name}.Application package AutoMapper", root);
            if (cqrs) ShellRunner.Run($"dotnet add {name}.Application package MediatR", root);

            if (db == "MongoDB")
            {
                ShellRunner.Run($"dotnet add {name}.Infrastructure package MongoDB.Driver", root);
            }
            else
            {
                ShellRunner.Run($"dotnet add {name}.Infrastructure package Microsoft.EntityFrameworkCore", root);
                ShellRunner.Run($"dotnet add {name}.Infrastructure package Microsoft.EntityFrameworkCore.Design", root);
                if (db == "Postgres") ShellRunner.Run($"dotnet add {name}.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL", root);
                if (db == "SQL Server") ShellRunner.Run($"dotnet add {name}.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer", root);
                if (db == "MySQL") ShellRunner.Run($"dotnet add {name}.Infrastructure package Pomelo.EntityFrameworkCore.MySql", root);
                if (db == "InMemory") ShellRunner.Run($"dotnet add {name}.Infrastructure package Microsoft.EntityFrameworkCore.InMemory", root);
            }

            if (swagger) ShellRunner.Run($"dotnet add {name}.Api package Swashbuckle.AspNetCore", root);

            var folders = new[]
            {
                Path.Combine(root,$"{name}.Domain","Entities"),
                Path.Combine(root,$"{name}.Domain","ValueObjects"),
                Path.Combine(root,$"{name}.Domain","Events"),
                Path.Combine(root,$"{name}.Domain","Services"),
                Path.Combine(root,$"{name}.Domain","Exceptions"),
                Path.Combine(root,$"{name}.Domain","Specifications"),
                Path.Combine(root,$"{name}.Application","Interfaces"),
                Path.Combine(root,$"{name}.Application","Services"),
                Path.Combine(root,$"{name}.Application","DTOs"),
                Path.Combine(root,$"{name}.Application","Commands"),
                Path.Combine(root,$"{name}.Application","Queries"),
                Path.Combine(root,$"{name}.Application","Handlers"),
                Path.Combine(root,$"{name}.Application","Validators"),
                Path.Combine(root,$"{name}.Application","Mapping"),
                Path.Combine(root,$"{name}.Application","UseCases"),
                Path.Combine(root,$"{name}.Infrastructure","Persistence"),
                Path.Combine(root,$"{name}.Infrastructure","Repositories"),
                Path.Combine(root,$"{name}.Infrastructure","Services"),
                Path.Combine(root,$"{name}.Infrastructure","Migrations"),
                Path.Combine(root,$"{name}.Infrastructure","Outbox"),
                Path.Combine(root,$"{name}.Infrastructure","EventBus"),
                Path.Combine(root,$"{name}.Infrastructure","Technical"),
                Path.Combine(root,$"{name}.Api","Controllers"),
                Path.Combine(root,$"{name}.Api","Middlewares"),
                Path.Combine(root,$"{name}.Api","Configurations"),
                Path.Combine(root,$"{name}.Api","Filters"),
                Path.Combine(root,$"{name}.Api","Dtos")
            };
            foreach (var d in folders) Directory.CreateDirectory(d);

            var appsettingsPath = Path.Combine(root, $"{name}.Api", "appsettings.json");
            var efConn = db switch
            {
                "Postgres" => $"Host=localhost;Port=5432;Database={name.ToLower()}_db;Username=postgres;Password=postgres",
                "SQL Server" => $"Server=localhost,1433;Database={name};User Id=sa;Password=Your_password123;TrustServerCertificate=True",
                "MySQL" => $"Server=localhost;Port=3306;Database={name.ToLower()}_db;User=root;Password=root",
                _ => ""
            };
            var json = db == "MongoDB"
                ? "{\n  \"Mongo\": {\"ConnectionString\": \"mongodb://localhost:27017\",\"Database\": \"" + name.ToLower() + "_db\"}\n}"
                : "{\n  \"ConnectionStrings\": {\"DefaultConnection\": \"" + efConn + "\"}\n}";
            FileHelper.WriteAllText(appsettingsPath, json);
        }
    }
}
