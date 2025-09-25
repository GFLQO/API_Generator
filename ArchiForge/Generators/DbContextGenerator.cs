using System.Linq;
using System.IO;
using ArchiForge.Utils;

namespace ArchiForge.Generators
{
    public static class DbContextGenerator
    {
        public static void Generate(string root, string project, string[] entities, string ddd)
        {
            var sets = string.Join("\n", entities.Select(x => $"        public Microsoft.EntityFrameworkCore.DbSet<{x}> {x}s {{ get; set; }}"));
            var code = $@"
using Microsoft.EntityFrameworkCore;
using {project}.Domain.Entities;

namespace {project}.Infrastructure.Persistence
{{
    public class AppDbContext : DbContext
    {{
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {{}}
{sets}
    }}
}}";
            FileHelper.WriteAllText(Path.Combine(root, $"{project}.Infrastructure", "Persistence", "AppDbContext.cs"), code);
        }
    }
}
