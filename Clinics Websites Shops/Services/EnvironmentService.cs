namespace Clinics_Websites_Shops.Services
{
    public class EnvironmentService
    {
        private readonly Dictionary<string, string> _environmentVariables;

        public EnvironmentService()
        {
            _environmentVariables = new Dictionary<string, string>();
            LoadEnvironmentFile();
        }

        private void LoadEnvironmentFile()
        {
            var envFilePath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
            
            if (!File.Exists(envFilePath))
            {
                // If .env doesn't exist, try to load from system environment variables
                return;
            }

            var lines = File.ReadAllLines(envFilePath);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                var parts = line.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();
                    
                    // Remove quotes if present
                    if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                        (value.StartsWith("'") && value.EndsWith("'")))
                    {
                        value = value.Substring(1, value.Length - 2);
                    }

                    _environmentVariables[key] = value;
                }
            }
        }

        public string GetValue(string key, string defaultValue = "")
        {
            // First check .env file variables
            if (_environmentVariables.ContainsKey(key))
            {
                return _environmentVariables[key];
            }

            // Then check system environment variables
            return Environment.GetEnvironmentVariable(key) ?? defaultValue;
        }

        public string GetConnectionString()
        {

            var dbType = GetValue("DB_CONNECTION", "sqlserver").ToLower();
            var host = GetValue("DB_HOST", "localhost");
            var port = GetValue("DB_PORT");
            var database = GetValue("DB_DATABASE");
            var username = GetValue("DB_USERNAME");
            var password = GetValue("DB_PASSWORD");
            return dbType switch
            {
                //"mysql" => BuildMySqlConnectionString(host, port, database, username, password),
                "sqlserver" => BuildSqlServerConnectionString(host, port, database, username, password),
                _ => throw new InvalidOperationException($"Unsupported database type: {dbType}")
            };
        }

        public string GetMasterConnectionString()
        {
            var dbType = GetValue("DB_CONNECTION", "sqlserver").ToLower();
            var host = GetValue("MASTER_DB_HOST", GetValue("DB_HOST", "localhost"));
            var port = GetValue("MASTER_DB_PORT", GetValue("DB_PORT"));
            var database = GetValue("MASTER_DB_DATABASE", "MasterDb");
            var username = GetValue("MASTER_DB_USERNAME", GetValue("DB_USERNAME"));
            var password = GetValue("MASTER_DB_PASSWORD", GetValue("DB_PASSWORD"));

            return dbType switch
            {
                //"mysql" => BuildMySqlConnectionString(host, port, database, username, password),
                "sqlserver" => BuildSqlServerConnectionString(host, port, database, username, password),
                _ => throw new InvalidOperationException($"Unsupported database type: {dbType}")
            };
        }

        private string BuildMySqlConnectionString(string host, string port, string database, string username, string password)
        {
            var portPart = string.IsNullOrEmpty(port) ? "" : $";Port={port}";
            var passwordPart = string.IsNullOrEmpty(password) ? "" : $";Password={password}";
            
            // For MySQL, if username is empty, try to connect without credentials (socket connection)
            if (string.IsNullOrEmpty(username))
            {
                return $"Server={host}{portPart};Database={database};SslMode=None;AllowPublicKeyRetrieval=true;";
            }
            
            return $"Server={host}{portPart};Database={database};User={username}{passwordPart};SslMode=None;AllowPublicKeyRetrieval=true;";
        }

        private string BuildSqlServerConnectionString(string host, string port, string database, string username, string password)
        {
            var serverPart = string.IsNullOrEmpty(port) ? host : $"{host},{port}";
            
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                // Use Windows Authentication
                return $"Server={serverPart};Database={database};Trusted_Connection=true;TrustServerCertificate=true;";
            }
            else
            {
                // Use SQL Server Authentication
                return $"Server={serverPart};Database={database};User Id={username};Password={password};TrustServerCertificate=true;";
            }
        }

        public DatabaseProvider GetDatabaseProvider()
        {
            var dbType = GetValue("DB_CONNECTION", "sqlserver").ToLower();
            return dbType switch
            {
                //"mysql" => DatabaseProvider.MySQL,
                "sqlserver" => DatabaseProvider.SqlServer,
                _ => throw new InvalidOperationException($"Unsupported database type: {dbType}")
            };
        }
    }

    public enum DatabaseProvider
    {
        SqlServer,
        MySQL
    }
}