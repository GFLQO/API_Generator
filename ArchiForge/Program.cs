using Spectre.Console;
using System;
using System.Diagnostics;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        // ASCII Banner
        AnsiConsole.Write(
            new FigletText("ARCHI").Centered().Color(Color.Red)
        );
        AnsiConsole.Write(
            new FigletText("FORGE").Centered().Color(Color.Blue)
        );

        AnsiConsole.MarkupLine("[bold yellow]Forge your Architecture[/]\n");

        var projectName = AnsiConsole.Ask<string>("Project [blue]name[/] ?");

        var db = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Choose your [green]Database[/]:")
                .AddChoices("Postgres", "SQL Server", "MySQL", "MongoDB", "InMemory")
        );

        var messaging = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Choose your [yellow]EventBus[/]:")
                .AddChoices("None", "Kafka", "RabbitMQ", "Azure Service Bus")
        );

        var cqrs = AnsiConsole.Confirm("Enable [blue]CQRS + MediatR[/] ?", true);
        var swagger = AnsiConsole.Confirm("Add [green]Swagger/OpenAPI[/] documentation ?", true);

        var ddd = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select [purple]DDD style[/]:")
                .AddChoices("Minimal", "Full")
        );

        var entities = AnsiConsole.Ask<string>("Base entities (comma separated) ?");
        var entityList = entities.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // Recap
        AnsiConsole.MarkupLine($"\n[bold]Configuration:[/]");
        AnsiConsole.MarkupLine($"Project : [blue]{projectName}[/]");
        AnsiConsole.MarkupLine($"Database : [green]{db}[/]");
        AnsiConsole.MarkupLine($"EventBus : [yellow]{messaging}[/]");
        AnsiConsole.MarkupLine($"CQRS : {(cqrs ? "[green]Yes[/]" : "[red]No[/]")}");
        AnsiConsole.MarkupLine($"Swagger : {(swagger ? "[green]Yes[/]" : "[red]No[/]")}");
        AnsiConsole.MarkupLine($"DDD : [purple]{ddd}[/]");
        AnsiConsole.MarkupLine($"Entities : {string.Join(", ", entityList)}");

        var generator = new ProjectGenerator(projectName, db, messaging, cqrs, swagger, ddd, entityList);
        generator.Generate();

        AnsiConsole.MarkupLine("\n[bold green]✅ Project successfully generated![/]");
        AnsiConsole.MarkupLine($"👉 cd forged/{projectName}.Api ; dotnet run");
    }
}

class ProjectGenerator
{
    private readonly string _name;
    private readonly string _db;
    private readonly string _messaging;
    private readonly bool _cqrs;
    private readonly bool _swagger;
    private readonly string _ddd;
    private readonly string[] _entities;

    public ProjectGenerator(string name, string db, string messaging, bool cqrs, bool swagger, string ddd, string[] entities)
    {
        _name = name;
        _db = db;
        _messaging = messaging;
        _cqrs = cqrs;
        _swagger = swagger;
        _ddd = ddd;
        _entities = entities;
    }

    public void Generate()
    {
        var binDir = AppContext.BaseDirectory;
        var solutionRoot = Directory.GetParent(binDir)!.Parent!.Parent!.Parent!.FullName;

        var forgedRoot = Path.Combine(solutionRoot, "forged");
        if (!Directory.Exists(forgedRoot))
            Directory.CreateDirectory(forgedRoot);

        var projectRoot = Path.Combine(forgedRoot, _name);
        if (!Directory.Exists(projectRoot))
            Directory.CreateDirectory(projectRoot);

        Directory.SetCurrentDirectory(projectRoot);

        Run($"dotnet new sln -n {_name}");

        // Domain
        Run($"dotnet new classlib -n {_name}.Domain");
        Run($"dotnet sln add {_name}.Domain/{_name}.Domain.csproj");

        // Application
        Run($"dotnet new classlib -n {_name}.Application");
        Run($"dotnet add {_name}.Application/{_name}.Application.csproj reference {_name}.Domain/{_name}.Domain.csproj");
        Run($"dotnet sln add {_name}.Application/{_name}.Application.csproj");

        // Infrastructure
        Run($"dotnet new classlib -n {_name}.Infrastructure");
        Run($"dotnet add {_name}.Infrastructure/{_name}.Infrastructure.csproj reference {_name}.Domain/{_name}.Domain.csproj");
        Run($"dotnet add {_name}.Infrastructure/{_name}.Infrastructure.csproj reference {_name}.Application/{_name}.Application.csproj");
        Run($"dotnet sln add {_name}.Infrastructure/{_name}.Infrastructure.csproj");

        // API
        Run($"dotnet new webapi -n {_name}.Api");
        Run($"dotnet add {_name}.Api/{_name}.Api.csproj reference {_name}.Application/{_name}.Application.csproj");
        Run($"dotnet add {_name}.Api/{_name}.Api.csproj reference {_name}.Infrastructure/{_name}.Infrastructure.csproj");
        Run($"dotnet sln add {_name}.Api/{_name}.Api.csproj");

        // Tests
        Run($"dotnet new xunit -n {_name}.Tests");
        Run($"dotnet sln add {_name}.Tests/{_name}.Tests.csproj");
        Run($"dotnet add {_name}.Tests/{_name}.Tests.csproj reference {_name}.Application/{_name}.Application.csproj");
        Run($"dotnet add {_name}.Tests/{_name}.Tests.csproj package FluentAssertions");

        // Packages
        if (_cqrs)
            Run($"dotnet add {_name}.Application package MediatR");

        Run($"dotnet add {_name}.Application package AutoMapper");

        if (_db == "MongoDB")
        {
            Run($"dotnet add {_name}.Infrastructure package MongoDB.Driver");
        }
        else
        {
            Run($"dotnet add {_name}.Infrastructure package Microsoft.EntityFrameworkCore");
            Run($"dotnet add {_name}.Infrastructure package Microsoft.EntityFrameworkCore.Design");

            if (_db == "Postgres")
                Run($"dotnet add {_name}.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL");

            if (_db == "SQL Server")
                Run($"dotnet add {_name}.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer");

            if (_db == "MySQL")
                Run($"dotnet add {_name}.Infrastructure package Pomelo.EntityFrameworkCore.MySql");

            if (_db == "InMemory")
                Run($"dotnet add {_name}.Infrastructure package Microsoft.EntityFrameworkCore.InMemory");
        }

        if (_swagger)
            Run($"dotnet add {_name}.Api package Swashbuckle.AspNetCore");

        // Folders
        string[] folders =
        {
            $"{_name}.Domain/Entities", $"{_name}.Domain/ValueObjects", $"{_name}.Domain/Events", $"{_name}.Domain/Services", $"{_name}.Domain/Exceptions", $"{_name}.Domain/Specifications",
            $"{_name}.Application/Interfaces", $"{_name}.Application/Services", $"{_name}.Application/DTOs", $"{_name}.Application/Commands", $"{_name}.Application/Queries", $"{_name}.Application/Handlers", $"{_name}.Application/Validators", $"{_name}.Application/Mapping", $"{_name}.Application/UseCases",
            $"{_name}.Infrastructure/Persistence", $"{_name}.Infrastructure/Repositories", $"{_name}.Infrastructure/Services", $"{_name}.Infrastructure/Migrations", $"{_name}.Infrastructure/Outbox", $"{_name}.Infrastructure/EventBus", $"{_name}.Infrastructure/Technical",
            $"{_name}.Api/Controllers", $"{_name}.Api/Middlewares", $"{_name}.Api/Configurations", $"{_name}.Api/Filters", $"{_name}.Api/Dtos"
        };
        foreach (var folder in folders)
            Directory.CreateDirectory(folder);

        // Entities
        foreach (var entity in _entities)
        {
            var code = $"namespace {_name}.Domain.Entities {{ public class {entity} {{ public int Id {{ get; set; }} }} }}";
            File.WriteAllText($"{_name}.Domain/Entities/{entity}.cs", code);
        }

        // Docker
        if (_db == "Postgres")
        {
            File.WriteAllText("docker-compose.yml", $@"
version: '3.9'
services:
  db:
    image: postgres:16
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: {_name.ToLower()}_db
    ports:
      - '5432:5432'
");
        }
    }

    private void Run(string cmd)
    {
        var psi = new ProcessStartInfo("cmd", $"/c {cmd}")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        var p = Process.Start(psi)!;
        p.WaitForExit();
    }
}
