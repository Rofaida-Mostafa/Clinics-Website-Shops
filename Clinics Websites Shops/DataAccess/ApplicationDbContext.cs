using Clinics_Websites_Shops.Services.IServices;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Clinics_Websites_Shops.DataAccess
{
    public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly ITenantService? _tenantService;
        private readonly IHttpContextAccessor? _httpContextAccessor;

        // ✅ Empty Constructor for design-time
        public ApplicationDbContext()
        {
        }

        // ✅ Constructor runtime
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ITenantService tenantService,
            IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _tenantService = tenantService;
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<Doctor> Doctors { get; set; } = null!;
        public DbSet<Nurse> Nurses { get; set; } = null!;
        public DbSet<Patient> Patients { get; set; } = null!;
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<Report> Reports { get; set; } = null!;
        public DbSet<Prescription> Prescriptions { get; set; } = null!;
        public DbSet<Evaluation> Evaluations { get; set; } = null!;

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
        }
    }

}
