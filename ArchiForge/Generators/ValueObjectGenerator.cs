using System.IO;
using ArchiForge.Utils;

namespace ArchiForge.Generators
{
    public static class ValueObjectGenerator
    {
        public static void Generate(string root, string project, string[] valueObjects)
        {
            var tpl = Path.Combine( "ValueObject.cs.txt");
            ShellRunner.Run($"dotnet add {project}.Domain/{project}.Domain.csproj package Ardalis.Specification", root);

            foreach (var vo in valueObjects)
            {
                var code = TemplateLoader.LoadAndReplace(tpl, ("ProjectName", project), ("ValueObjectName", vo));
                FileHelper.WriteAllText(Path.Combine(root, $"{project}.Domain", "ValueObjects", $"{vo}.cs"), code);
            }
        }
    }
}
