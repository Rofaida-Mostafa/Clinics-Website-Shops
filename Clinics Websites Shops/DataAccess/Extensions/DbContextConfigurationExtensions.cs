using Microsoft.EntityFrameworkCore;
using Clinics_Websites_Shops.Services;

namespace Clinics_Websites_Shops.DataAccess.Extensions
{
    public static class DbContextConfigurationExtensions
    {
        public static void ConfigureDatabase(this DbContextOptionsBuilder optionsBuilder, 
            string connectionString, 
            DatabaseProvider provider)
        {
            switch (provider)
            {
                case DatabaseProvider.SqlServer:
                    optionsBuilder.UseSqlServer(connectionString, options =>
                    {
                        options.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null);
                        options.CommandTimeout(60);
                    });
                    break;

                case DatabaseProvider.MySQL:
                    optionsBuilder.UseMySql(connectionString, 
                        ServerVersion.AutoDetect(connectionString), options =>
                    {
                        options.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null);
                        options.CommandTimeout(60);
                    });
                    break;

                default:
                    throw new ArgumentException($"Unsupported database provider: {provider}");
            }

            // Common configurations
            optionsBuilder.EnableSensitiveDataLogging(false);
            optionsBuilder.EnableDetailedErrors(true);
        }

        public static void ApplyDatabaseSpecificConfigurations(this ModelBuilder modelBuilder, 
            DatabaseProvider provider)
        {
            switch (provider)
            {
                case DatabaseProvider.SqlServer:
                    ApplySqlServerSpecificConfigurations(modelBuilder);
                    break;

                case DatabaseProvider.MySQL:
                    ApplyMySqlSpecificConfigurations(modelBuilder);
                    break;
            }
        }

        private static void ApplySqlServerSpecificConfigurations(ModelBuilder modelBuilder)
        {
            // SQL Server specific configurations
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // Configure datetime properties for SQL Server
                foreach (var property in entity.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetColumnType("datetime2");
                    }
                }
            }
        }

        private static void ApplyMySqlSpecificConfigurations(ModelBuilder modelBuilder)
        {
            // MySQL specific configurations
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // Configure string properties for MySQL
                foreach (var property in entity.GetProperties())
                {
                    if (property.ClrType == typeof(string))
                    {
                        // Set default charset for string properties in MySQL
                        property.SetAnnotation("MySql:CharSet", "utf8mb4");
                        property.SetAnnotation("MySql:Collation", "utf8mb4_unicode_ci");
                    }
                    
                    // Configure boolean properties for MySQL
                    if (property.ClrType == typeof(bool) || property.ClrType == typeof(bool?))
                    {
                        property.SetColumnType("tinyint(1)");
                    }
                }

                // Configure table names to use lowercase for MySQL conventions
                var tableName = entity.GetTableName();
                if (!string.IsNullOrEmpty(tableName))
                {
                    entity.SetTableName(tableName.ToLowerInvariant());
                }
            }
        }
    }
}