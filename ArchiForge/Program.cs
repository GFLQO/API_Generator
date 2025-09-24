using Spectre.Console;
using ArchiForge.Generators;
using System;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Banner ASCII (ARCHI en rouge + FORGE en bleu, côte à côte)
        var archi = new FigletText("ARCHI").Color(Color.Red);
        var forge = new FigletText("FORGE").Color(Color.Blue);
        AnsiConsole.Write(new Columns(archi, forge));

        AnsiConsole.MarkupLine("[bold yellow]Forge your Architecture[/]\n");

        // User inputs
        var projectName = AnsiConsole.Ask<string>("Project [blue]name[/] ?");
        var db = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Choose your [green]Database[/]:")
            .AddChoices("Postgres", "SQL Server", "MySQL", "MongoDB", "InMemory"));
        var messaging = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Choose your [yellow]EventBus[/]:")
            .AddChoices("None", "Kafka", "RabbitMQ", "Azure Service Bus"));
        var cqrs = AnsiConsole.Confirm("Enable [blue]CQRS + MediatR[/] ?", true);
        var swagger = AnsiConsole.Confirm("Add [green]Swagger/OpenAPI[/] documentation ?", true);
        var ddd = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Select [purple]DDD style[/]:")
            .AddChoices("Minimal", "Full"));
        var entities = AnsiConsole.Ask<string>("Base entities (comma separated) ?");
        var entityList = entities.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // Root project path Desktop/ArchiForged
        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var forgedRoot = Path.Combine(desktop, "ArchiForged");
        Directory.CreateDirectory(forgedRoot);
        var projectRoot = Path.Combine(forgedRoot, projectName);
        Directory.CreateDirectory(projectRoot);

        // ================= PROGRESS BAR =================
        await AnsiConsole.Progress()
            .AutoClear(true)
            .HideCompleted(false)
            .Columns(new ProgressColumn[]
            {
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new ElapsedTimeColumn(),
                new SpinnerColumn()
            })
            .StartAsync(async ctx =>
            {
                var t1 = ctx.AddTask("[green]Solution[/]");
                var t2 = ctx.AddTask("[green]Entities[/]");
                var t3 = db != "MongoDB" ? ctx.AddTask("[green]DbContext[/]") : null;
                var t4 = cqrs ? ctx.AddTask("[green]CQRS[/]") : null;
                var t5 = ctx.AddTask("[green]Docker[/]");
                var t6 = ctx.AddTask("[green]Messaging[/]");
                var t7 = ctx.AddTask("[green]Swagger[/]");

                // Execution asynchrone
                await Task.Run(() => SolutionGenerator.Generate(projectRoot, projectName, db, cqrs, swagger, ddd, entityList));
                t1.Increment(100);

                await Task.Run(() => EntityGenerator.Generate(projectRoot, projectName, entityList));
                t2.Increment(100);

                if (t3 != null)
                {
                    await Task.Run(() => DbContextGenerator.Generate(projectRoot, projectName, entityList));
                    t3.Increment(100);
                }

                if (t4 != null)
                {
                    await Task.Run(() => CqrsGenerator.Generate(projectRoot, projectName, entityList));
                    t4.Increment(100);
                }

                await Task.Run(() => DockerGenerator.Generate(projectRoot, projectName, db));
                t5.Increment(100);

                await Task.Run(() => MessagingGenerator.Generate(projectRoot, projectName, messaging));
                t6.Increment(100);

                await Task.Run(() => SwaggerGenerator.Ensure(projectRoot, projectName, swagger));
                t7.Increment(100);
            });

        // ================= RECAP =================
        var apiPath = Path.Combine(projectRoot, $"{projectName}.Api");
        AnsiConsole.MarkupLine("\n[bold green]🚀 Project fully generated![/]");
        AnsiConsole.MarkupLine($"📂 Location: [blue]{projectRoot}[/]");
        AnsiConsole.MarkupLine($"👉 cd \"{apiPath}\" ; dotnet run");
    }
}
