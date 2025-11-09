using Clinics_Websites_Shops.Services.IServices;
using Clinics_Websites_Shops.Services;
using Clinics_Websites_Shops.Settings;
using Clinics_Websites_Shops.DataAccess.Extensions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Clinics_Websites_Shops.DataAccess
{
    public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly ITenantService? _tenantService;
        private readonly IHttpContextAccessor? _httpContextAccessor;
        private readonly EnvironmentService? _environmentService;
        public string? TenantId { get; set; }

        // ✅ Empty Constructor for design-time
        public ApplicationDbContext()
        {
            _environmentService = new EnvironmentService();
        }

        // ✅ Constructor runtime
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ITenantService tenantService,
            IHttpContextAccessor httpContextAccessor,
            EnvironmentService environmentService)
            : base(options)
        {
            _tenantService = tenantService;
            _httpContextAccessor = httpContextAccessor;
            _environmentService = environmentService;

            if (_tenantService != null && _httpContextAccessor?.HttpContext != null)
            {
                TenantId = _tenantService.GetCurrentTenant(_httpContextAccessor.HttpContext)?.TId;
            }
        }

        public DbSet<Doctor> Doctors { get; set; } = null!;
        public DbSet<Nurse> Nurses { get; set; } = null!;
        public DbSet<Patient> Patients { get; set; } = null!;
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<DepartmentTranslation> DepartmentTranslations { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<Report> Reports { get; set; } = null!;
        public DbSet<Prescription> Prescriptions { get; set; } = null!;
        public DbSet<Evaluation> Evaluations { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var environmentService = _environmentService ?? new EnvironmentService();
                var connectionString = string.Empty;
                var databaseProvider = environmentService.GetDatabaseProvider();

                // Using Tenant Service to get the connection string dynamically
                if (_tenantService != null && _httpContextAccessor?.HttpContext != null)
                {
                    var tenant = _tenantService.GetCurrentTenant(_httpContextAccessor.HttpContext);

                    if (tenant != null && !string.IsNullOrEmpty(tenant.ConnectionString))
                    {
                        connectionString = tenant.ConnectionString;
                    }
                    else
                    {
                        // Fallback to environment configuration
                        connectionString = environmentService.GetConnectionString();
                    }
                }
                else
                {
                    // For design-time and migrations
                    connectionString = environmentService.GetConnectionString();
                }

                // Configure the database provider
                optionsBuilder.ConfigureDatabase(connectionString, databaseProvider);
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUser>().HasQueryFilter(u => u.TenantId == TenantId);
          
            base.OnModelCreating(modelBuilder);

            // Apply database-specific configurations
            var environmentService = _environmentService ?? new EnvironmentService();
            var databaseProvider = environmentService.GetDatabaseProvider();
            modelBuilder.ApplyDatabaseSpecificConfigurations(databaseProvider);

            // Person primary key
            modelBuilder.Entity<ApplicationUser>().HasKey(p => p.Id);

            // Doctor, Patient, Nurse link to Person via ApplicationUserId
            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.ApplicationUser)
                .WithMany()
                .HasForeignKey(d => d.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Patient>()
                .HasOne(p => p.ApplicationUser)
                .WithMany()
                .HasForeignKey(p => p.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Nurse>()
                .HasOne(n => n.ApplicationUser)
                .WithMany()
                .HasForeignKey(n => n.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Doctor - Appointment
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Patient - Appointment
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment - Appointment (1:1)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Appointment)
                .WithOne(a => a.Payment)
                .HasForeignKey<Payment>(p => p.AppointmentId);

            // Report relations
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Patient)
                .WithMany(p => p.Reports)
                .HasForeignKey(r => r.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.Doctor)
                .WithMany(d => d.Reports)
                .HasForeignKey(r => r.DoctorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Prescription -> Report
            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Report)
                .WithMany(r => r.Prescriptions)
                .HasForeignKey(p => p.ReportId)
                .OnDelete(DeleteBehavior.Cascade);

            // Department - Doctor
            modelBuilder.Entity<Department>()
                .HasMany(d => d.Doctors)
                .WithOne()
                .HasForeignKey("DepartmentId")
                .OnDelete(DeleteBehavior.SetNull);

            // Department - DepartmentTranslation
            modelBuilder.Entity<DepartmentTranslation>()
                .HasKey(dt => dt.Id);

            modelBuilder.Entity<DepartmentTranslation>()
                .HasOne(dt => dt.Department)
                .WithMany(d => d.Translations)
                .HasForeignKey(dt => dt.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DepartmentTranslation>()
                .HasIndex(dt => new { dt.DepartmentId, dt.LanguageCode })
                .IsUnique();

            modelBuilder.Entity<DepartmentTranslation>()
                .Property(dt => dt.LanguageCode)
                .HasMaxLength(10)
                .IsRequired();

            modelBuilder.Entity<DepartmentTranslation>()
                .Property(dt => dt.Name)
                .HasMaxLength(200)
                .IsRequired();
        }
    }

}
