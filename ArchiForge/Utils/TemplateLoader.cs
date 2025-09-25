using System;
using System.IO;

namespace ArchiForge.Utils
{
    public static class TemplateLoader
    {
        public static string LoadAndReplace(string templateFile, params (string Key, string Value)[] replacements)
        {
            // Base path = racine du projet (pas bin/Debug)
            var basePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\"));
            var fullPath = Path.Combine(basePath, "Templates", templateFile);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Template introuvable : {fullPath}");

            var template = File.ReadAllText(fullPath);

            foreach (var (k, v) in replacements)
                template = template.Replace("{{" + k + "}}", v);

            return template;
        }
    }
}
