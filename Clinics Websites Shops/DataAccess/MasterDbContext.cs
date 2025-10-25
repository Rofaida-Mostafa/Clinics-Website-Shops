using Microsoft.EntityFrameworkCore;

namespace Clinics_Websites_Shops.DataAccess
{
    public class MasterDbContext : DbContext
    {
        public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options) { }

        public DbSet<Tenant> Tenants { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Tenant>(entity =>
            {
                entity.HasKey(e => e.TId);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Domain).IsRequired();
                entity.Property(e => e.Status).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}
