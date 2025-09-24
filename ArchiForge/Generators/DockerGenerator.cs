using System.IO;

namespace ArchiForge.Generators
{
    public static class DockerGenerator
    {
        public static void Generate(string projectRoot, string projectName, string db)
        {
            string dockerContent = db switch
            {
                "Postgres" => $@"services:
  db:
    image: postgres:16
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: {projectName.ToLower()}_db
    ports:
      - '5432:5432'
    volumes:
      - pgdata:/var/lib/postgresql/data
volumes:
  pgdata:",
                "SQL Server" => $@"services:
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: Your_password123
      ACCEPT_EULA: Y
    ports:
      - '1433:1433'
    volumes:
      - mssqldata:/var/opt/mssql
volumes:
  mssqldata:",
                "MySQL" => $@"services:
  db:
    image: mysql:8.0
    environment:
      MYSQL_ROOT_PASSWORD: root
      MYSQL_DATABASE: {projectName.ToLower()}_db
    ports:
      - '3306:3306'
    volumes:
      - mysqldata:/var/lib/mysql
volumes:
  mysqldata:",
                "MongoDB" => $@"services:
  mongo:
    image: mongo:7.0
    container_name: {projectName.ToLower()}_mongo
    ports:
      - '27017:27017'
    volumes:
      - mongodata:/data/db
volumes:
  mongodata:",
                _ => ""
            };

            if (!string.IsNullOrEmpty(dockerContent))
                File.WriteAllText(Path.Combine(projectRoot, "docker-compose.yml"), dockerContent);
        }
    }
}
