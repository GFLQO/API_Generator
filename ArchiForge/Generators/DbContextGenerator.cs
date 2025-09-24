using System.IO;
using System.Linq;

namespace ArchiForge.Generators
{
    public static class DbContextGenerator
    {
        public static void Generate(string projectRoot, string projectName, string[] entities)
        {
            var dbSets = string.Join("\n",
                entities.Select(e => $"        public DbSet<{e}> {e}s {{ get; set; }}"));

            var code = $@"
using Microsoft.EntityFrameworkCore;
using {projectName}.Domain.Entities;

namespace {projectName}.Infrastructure.Persistence
{{
    public class AppDbContext : DbContext
    {{
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {{ }}

{dbSets}
    }}
}}";
            var file = Path.Combine(projectRoot, $"{projectName}.Infrastructure", "Persistence", "AppDbContext.cs");
            File.WriteAllText(file, code);
        }
    }
}
