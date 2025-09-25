using System.IO;
using ArchiForge.Utils;

namespace ArchiForge.Generators
{
    public static class CqrsGenerator
    {
        public static void Generate(string root, string project, string[] entities, string ddd)
        {
            foreach (var e in entities)
            {
                var cmd = $@"
using MediatR;

namespace {project}.Application.Commands
{{
    public record Create{e}Command(string Name) : IRequest<string>;
}}";
                FileHelper.WriteAllText(Path.Combine(root, $"{project}.Application", "Commands", $"Create{e}Command.cs"), cmd);

                var h = ddd == "Full"
                    ? $@"
using MediatR;
using {project}.Application.Interfaces;
using {project}.Application.Commands;
using {project}.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace {project}.Application.Handlers
{{
    public class Create{e}Handler : IRequestHandler<Create{e}Command, string>
    {{
        private readonly I{e}Repository _repo;
        public Create{e}Handler(I{e}Repository repo) => _repo = repo;
        public async Task<string> Handle(Create{e}Command request, CancellationToken ct)
        {{
            var entity = new {e}();
            await _repo.AddAsync(entity);
            return entity.Id.ToString();
        }}
    }}
}}"
                    :
                      $@"
using MediatR;
using {project}.Application.Interfaces;
using {project}.Application.Commands;
using {project}.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace {project}.Application.Handlers
{{
    public class Create{e}Handler : IRequestHandler<Create{e}Command, string>
    {{
        private readonly I{e}Repository _repo;
        public Create{e}Handler(I{e}Repository repo) => _repo = repo;
        public async Task<string> Handle(Create{e}Command request, CancellationToken ct)
        {{
            var entity = new {e}();
            await _repo.AddAsync(entity);
            return entity.Id.ToString();
        }}
    }}
}}";
                FileHelper.WriteAllText(Path.Combine(root, $"{project}.Application", "Handlers", $"Create{e}Handler.cs"), h);
            }
        }
    }
}
