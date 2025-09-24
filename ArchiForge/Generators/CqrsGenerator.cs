using System.IO;

namespace ArchiForge.Generators
{
    public static class CqrsGenerator
    {
        public static void Generate(string projectRoot, string projectName, string[] entities)
        {
            foreach (var entity in entities)
            {
                var commandCode = $@"
using MediatR;

namespace {projectName}.Application.Commands
{{
    public record Create{entity}Command(string Name) : IRequest<int>;
}}";
                File.WriteAllText(Path.Combine(projectRoot, $"{projectName}.Application", "Commands", $"Create{entity}Command.cs"), commandCode);

                var handlerCode = $@"
using MediatR;
using {projectName}.Domain.Entities;

namespace {projectName}.Application.Handlers
{{
    public class Create{entity}Handler : IRequestHandler<Create{entity}Command, int>
    {{
        public Task<int> Handle(Create{entity}Command request, CancellationToken cancellationToken)
        {{
            // TODO: persist entity
            return Task.FromResult(1);
        }}
    }}
}}";
                File.WriteAllText(Path.Combine(projectRoot, $"{projectName}.Application", "Handlers", $"Create{entity}Handler.cs"), handlerCode);
            }
        }
    }
}
