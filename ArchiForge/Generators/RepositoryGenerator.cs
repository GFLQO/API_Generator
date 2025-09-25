using System.IO;
using ArchiForge.Utils;

namespace ArchiForge.Generators
{
    public static class RepositoryGenerator
    {
        public static void Generate(string root, string project, string[] entities, string db, string ddd)
        {
            foreach (var e in entities)
            {
                var idType = db == "MongoDB" ? "string" : "int";

                var ifaceTpl = Path.Combine( ddd == "Full" ? "Repository.Aggregate.Interface.cs.txt" : "Repository.Crud.Interface.cs.txt");
                var ifaceCode = TemplateLoader.LoadAndReplace(ifaceTpl, ("ProjectName", project), ("EntityName", e), ("IdType", idType));
                FileHelper.WriteAllText(Path.Combine(root, $"{project}.Application", "Interfaces", $"I{e}Repository.cs"), ifaceCode);

                var implTpl = Path.Combine(
                    ddd == "Full"
                        ? (db == "MongoDB" ? "Repository.Aggregate.Impl.Mongo.cs.txt" : "Repository.Aggregate.Impl.EF.cs.txt")
                        : (db == "MongoDB" ? "Repository.Crud.Impl.Mongo.cs.txt" : "Repository.Crud.Impl.EF.cs.txt"));

                var implCode = TemplateLoader.LoadAndReplace(implTpl, ("ProjectName", project), ("EntityName", e), ("IdType", idType));
                FileHelper.WriteAllText(Path.Combine(root, $"{project}.Infrastructure", "Repositories", $"{e}Repository.cs"), implCode);
            }
        }
    }
}
