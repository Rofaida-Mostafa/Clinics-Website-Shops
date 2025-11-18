using Clinics_Websites_Shops.Services.IServices;
using Clinics_Websites_Shops.Services;
using Clinics_Websites_Shops.Settings;
using Clinics_Websites_Shops.DataAccess.Extensions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Clinics_Websites_Shops.DataAccess
{
    public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
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
    
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
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
        public DbSet<MedicalResult> MedicalResults { get; set; } = null!;
        public DbSet<ClinicSettings> ClinicSettings { get; set; } = null!;
        public DbSet<ClinicLocation> ClinicLocations { get; set; } = null!;
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; } = null!;
        public DbSet<DoctorHoliday> DoctorHolidays { get; set; } = null!;

        // Permission System
        public DbSet<Permission> Permissions { get; set; } = null!;
        public DbSet<RolePermission> RolePermissions { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
               
                    // Using Tenant Service to get the connection string dynamically
                    if (_tenantService != null && _httpContextAccessor != null)
                    {
                        var tenant = _tenantService.GetCurrentTenant(_httpContextAccessor.HttpContext);

                        if (tenant == null)
                            throw new Exception("Tenant not found for the current request");

                        optionsBuilder.UseSqlServer(tenant.ConnectionString);
                    }
                    else
                    {
                        //  Implement DB using EF core ( For Migration only)
                        optionsBuilder.UseSqlServer("Server=.;Database=ClinicOneDb;Trusted_Connection=True;TrustServerCertificate=True;");
                    }
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
            modelBuilder.Entity<ApplicationUser>().HasQueryFilter(u => u.TenantId == TenantId);

            base.OnModelCreating(modelBuilder);
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

            // Configure unique indexes for business keys
            modelBuilder.Entity<Doctor>()
                .HasIndex(d => d.DoctorId)
                .IsUnique();

            modelBuilder.Entity<Nurse>()
                .HasIndex(n => n.NurseId)
                .IsUnique();

            modelBuilder.Entity<Patient>()
                .HasIndex(p => p.PatientNumber)
                .IsUnique();

            modelBuilder.Entity<Appointment>()
                .HasIndex(a => a.AppointmentNumber)
                .IsUnique();

            // Doctor - Appointment (using Id as foreign key)
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Patient - Appointment (using Id as foreign key)
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Department - Appointment (optional relationship)
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Department)
                .WithMany()
                .HasForeignKey(a => a.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

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

            // Prescription relationships
            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Report)
                .WithMany(r => r.Prescriptions)
                .HasForeignKey(p => p.ReportId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Patient)
                .WithMany(pat => pat.Prescriptions)
                .HasForeignKey(p => p.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Doctor)
                .WithMany()
                .HasForeignKey(p => p.DoctorId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Appointment)
                .WithMany()
                .HasForeignKey(p => p.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Prescription>()
                .HasIndex(p => p.PrescriptionNumber)
                .IsUnique();

            // Explicitly configure Prescription.PatientId as the foreign key
            modelBuilder.Entity<Prescription>()
                .Property(p => p.PatientId)
                .IsRequired();

            // MedicalResult relationships
            modelBuilder.Entity<MedicalResult>()
                .HasOne(mr => mr.Patient)
                .WithMany(p => p.MedicalResults)
                .HasForeignKey(mr => mr.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MedicalResult>()
                .HasOne(mr => mr.Doctor)
                .WithMany()
                .HasForeignKey(mr => mr.DoctorId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<MedicalResult>()
                .HasOne(mr => mr.Appointment)
                .WithMany()
                .HasForeignKey(mr => mr.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<MedicalResult>()
                .HasOne(mr => mr.Report)
                .WithMany()
                .HasForeignKey(mr => mr.ReportId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<MedicalResult>()
                .HasIndex(mr => mr.ResultNumber)
                .IsUnique();

            // Explicitly configure MedicalResult.PatientId as the foreign key
            modelBuilder.Entity<MedicalResult>()
                .Property(mr => mr.PatientId)
                .IsRequired();

            // ClinicSettings - One per tenant
            modelBuilder.Entity<ClinicSettings>()
                .HasIndex(cs => cs.TenantId)
                .IsUnique();

            // Department - Doctor
            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.Department)
                .WithMany(dept => dept.Doctors)
                .HasForeignKey(d => d.DepartmentId)
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

            // Permission System Configuration
            modelBuilder.Entity<Permission>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Permission>()
                .HasIndex(p => p.Name)
                .IsUnique();

            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => rp.Id);

            modelBuilder.Entity<RolePermission>()
                .HasIndex(rp => new { rp.RoleId, rp.PermissionId })
                .IsUnique();

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
