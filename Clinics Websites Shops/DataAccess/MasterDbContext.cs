using Microsoft.EntityFrameworkCore;
using Clinics_Websites_Shops.Services;
using Clinics_Websites_Shops.DataAccess.Extensions;

namespace Clinics_Websites_Shops.DataAccess
{
    public class MasterDbContext : DbContext
    {
        private readonly EnvironmentService? _environmentService;

        // Design-time constructor
        public MasterDbContext() 
        {
            _environmentService = new EnvironmentService();
        }

        // Runtime constructor
        public MasterDbContext(DbContextOptions<MasterDbContext> options, EnvironmentService environmentService) 
            : base(options) 
        {
            _environmentService = environmentService;
        }

        public DbSet<Tenant> Tenants { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var environmentService = _environmentService ?? new EnvironmentService();
                var connectionString = environmentService.GetMasterConnectionString();
                var databaseProvider = environmentService.GetDatabaseProvider();
                
                optionsBuilder.ConfigureDatabase(connectionString, databaseProvider);
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Apply database-specific configurations
            var environmentService = _environmentService ?? new EnvironmentService();
            var databaseProvider = environmentService.GetDatabaseProvider();
            builder.ApplyDatabaseSpecificConfigurations(databaseProvider);

            builder.Entity<Tenant>(entity =>
            {
                entity.HasKey(e => e.TId);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Domain).IsRequired();
                entity.Property(e => e.Status).HasDefaultValue(true);
                
                // Database-specific default value configurations
                if (databaseProvider == DatabaseProvider.SqlServer)
                {
                    entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                }
                else if (databaseProvider == DatabaseProvider.MySQL)
                {
                    entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
                }
            });
        }
    }
}
