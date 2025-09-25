using System.IO;
using ArchiForge.Utils;

namespace ArchiForge.Generators
{
    public static class ControllerGenerator
    {
        public static void Generate(string root, string project, string[] entities, string db, string ddd)
        {
            var tpl = Path.Combine( ddd == "Full" ? "Controller.Full.cs.txt" : "Controller.Minimal.cs.txt");
            var idType = db == "MongoDB" ? "string" : "int";

            foreach (var e in entities)
            {
                var code = TemplateLoader.LoadAndReplace(tpl, ("ProjectName", project), ("EntityName", e), ("IdType", idType));
                FileHelper.WriteAllText(Path.Combine(root, $"{project}.Api", "Controllers", $"{e}Controller.cs"), code);
            }
        }
    }
}
