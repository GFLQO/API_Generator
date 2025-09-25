using System.IO;
using ArchiForge.Utils;

namespace ArchiForge.Generators
{
    public static class EntityGenerator
    {
        public static void Generate(string root, string project, string[] entities, string db, string ddd)
        {
            var template = Path.Combine(
                ddd == "Full" ? "Entity.Full.cs.txt" : "Entity.Minimal.cs.txt"
            );

            var idDecl = db == "MongoDB"
                ? "[BsonId]\n        [BsonRepresentation(BsonType.ObjectId)]\n        public string Id { get; set; } = string.Empty;"
                : "public int Id { get; set; }";

            if (db == "MongoDB")
            {
                ShellRunner.Run($"dotnet add {project}.Domain/{project}.Domain.csproj package MongoDB.Bson", root);
            }



            foreach (var e in entities)
            {
                var code = TemplateLoader.LoadAndReplace(template,
                    ("ProjectName", project),
                    ("EntityName", e),
                    ("IdDeclaration", idDecl)
                );

                if (db == "MongoDB")
                {
                    code = "using MongoDB.Bson;\nusing MongoDB.Bson.Serialization.Attributes;\n\n" + code;
                }

                FileHelper.WriteAllText(
                    Path.Combine(root, $"{project}.Domain", "Entities", $"{e}.cs"),
                    code
                );
            }
        }
    }
}
