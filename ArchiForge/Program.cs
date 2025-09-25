using Spectre.Console;
using ArchiForge.Generators;
using System;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var archi = new FigletText("ARCHI").Color(Color.Red);
        var forge = new FigletText("FORGE").Color(Color.Blue);
        AnsiConsole.Write(new Columns(archi, forge));
        AnsiConsole.MarkupLine("[bold yellow]Forge your Architecture[/]\n");

        var projectName = AnsiConsole.Ask<string>("Project [blue]name[/] ?");
        var db = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Choose your [green]Database[/]:").AddChoices("Postgres", "SQL Server", "MySQL", "MongoDB", "InMemory"));
        var messaging = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Choose your [yellow]EventBus[/]:").AddChoices("None", "Kafka", "RabbitMQ", "Azure Service Bus"));
        var cqrs = AnsiConsole.Confirm("Enable [blue]CQRS + MediatR[/] ?", true);
        var swagger = AnsiConsole.Confirm("Add [green]Swagger/OpenAPI[/] documentation ?", true);
        var ddd = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Select [purple]DDD style[/]:").AddChoices("Minimal", "Full"));
        var entities = AnsiConsole.Ask<string>("Base entities (comma separated) ?");
        var entityList = entities.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var forgedRoot = Path.Combine(desktop, "ArchiForged");
        Directory.CreateDirectory(forgedRoot);
        var projectRoot = Path.Combine(forgedRoot, projectName);
        Directory.CreateDirectory(projectRoot);

        await AnsiConsole.Progress()
            .AutoClear(false)
            .HideCompleted(false)
            .Columns(new ProgressColumn[] { new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new ElapsedTimeColumn(), new SpinnerColumn() })
            .StartAsync(async ctx =>
            {
                var t1 = ctx.AddTask("[green]Solution[/]");
                await Task.Run(() => SolutionGenerator.Generate(projectRoot, projectName, db, cqrs, swagger, ddd, entityList));
                t1.Increment(100);

                var t2 = ctx.AddTask("[green]Entities[/]");
                await Task.Run(() => EntityGenerator.Generate(projectRoot, projectName, entityList, db, ddd));
                t2.Increment(100);

                var t3 = db != "MongoDB" ? ctx.AddTask("[green]DbContext[/]") : null;
                if (t3 != null)
                {
                    await Task.Run(() => DbContextGenerator.Generate(projectRoot, projectName, entityList, ddd));
                    t3.Increment(100);
                }

                var tRepo = ctx.AddTask("[green]Repositories[/]");
                await Task.Run(() => RepositoryGenerator.Generate(projectRoot, projectName, entityList, db, ddd));
                tRepo.Increment(100);

                var tCtrl = ctx.AddTask("[green]Controllers[/]");
                await Task.Run(() => ControllerGenerator.Generate(projectRoot, projectName, entityList, db, ddd));
                tCtrl.Increment(100);

                if (cqrs)
                {
                    var t4 = ctx.AddTask("[green]CQRS[/]");
                    await Task.Run(() => CqrsGenerator.Generate(projectRoot, projectName, entityList, ddd));
                    t4.Increment(100);
                }

                var tVal = ddd == "Full" ? ctx.AddTask("[green]Value Objects[/]") : null;
                if (tVal != null)
                {
                    await Task.Run(() => ValueObjectGenerator.Generate(projectRoot, projectName, Array.Empty<string>()));
                    tVal.Increment(100);
                }

                var t5 = ctx.AddTask("[green]Docker[/]");
                await Task.Run(() => DockerGenerator.Generate(projectRoot, projectName, db));
                t5.Increment(100);

                var t6 = ctx.AddTask("[green]Messaging[/]");
                await Task.Run(() => MessagingGenerator.Generate(projectRoot, projectName, messaging));
                t6.Increment(100);

                var t7 = ctx.AddTask("[green]Swagger[/]");
                await Task.Run(() => SwaggerGenerator.Ensure(projectRoot, projectName, swagger));
                t7.Increment(100);
            });

        var apiPath = Path.Combine(projectRoot, $"{projectName}.Api");
        AnsiConsole.MarkupLine("\n[bold green]🚀 Project fully generated![/]");
        AnsiConsole.MarkupLine($"📂 Location: [blue]{projectRoot}[/]");
        AnsiConsole.MarkupLine($"👉 cd \"{apiPath}\" ; dotnet run");
    }
}
