using System.IO;

namespace ArchiForge.Generators
{
    public static class EntityGenerator
    {
        public static void Generate(string projectRoot, string projectName, string[] entities)
        {
            foreach (var entity in entities)
            {
                var code = $@"
namespace {projectName}.Domain.Entities
{{
    public class {entity}
    {{
        public int Id {{ get; set; }}
    }}
}}";
                var file = Path.Combine(projectRoot, $"{projectName}.Domain", "Entities", $"{entity}.cs");
                File.WriteAllText(file, code);
            }
        }
    }
}
