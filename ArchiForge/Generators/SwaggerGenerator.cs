using ArchiForge.Utils;
using System.IO;

namespace ArchiForge.Generators
{
    public static class SwaggerGenerator
    {
        public static void Ensure(string root, string name, bool enabled)
        {
            if (!enabled) return;
            var programPath = Path.Combine(root, $"{name}.Api", "Program.cs");
            if (!File.Exists(programPath)) return;
            var txt = File.ReadAllText(programPath);

            if (!txt.Contains("AddEndpointsApiExplorer()")) txt = txt.Replace("builder.Services.AddControllers();", "builder.Services.AddControllers();\nbuilder.Services.AddEndpointsApiExplorer();");
            if (!txt.Contains("AddSwaggerGen()")) txt = txt.Replace("builder.Services.AddControllers();", "builder.Services.AddControllers();\nbuilder.Services.AddSwaggerGen();");

            if (!txt.Contains("UseSwaggerUI()"))
            {
                txt = txt.Replace("app.UseAuthorization();", "app.UseAuthorization();\nif (app.Environment.IsDevelopment())\n{\n    app.UseSwagger();\n    app.UseSwaggerUI();\n}");
            }

            FileHelper.WriteAllText(programPath, txt);
        }
    }
}
