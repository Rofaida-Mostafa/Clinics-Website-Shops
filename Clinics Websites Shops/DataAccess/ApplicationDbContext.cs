using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Clinics_Websites_Shops.DataAccess
{
    public partial class ApplicationDbContext : IdentityDbContext<Person>
    {
        private readonly ITenantService? _tenantService;
        private readonly IHttpContextAccessor? _httpContextAccessor;

        // ✅ Constructor فارغ للـ design-time (EF Tools)
        public ApplicationDbContext()
        {
        }

        // ✅ Constructor runtime (لما التطبيق شغال)
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

            // Person inheritance
            modelBuilder.Entity<Person>().HasKey(p => p.Id);
            modelBuilder.Entity<Doctor>().HasBaseType<Person>();
            modelBuilder.Entity<Nurse>().HasBaseType<Person>();
            modelBuilder.Entity<Patient>().HasBaseType<Person>();

            // Doctor - Appointment
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor).WithMany(d => d.Appointments).HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient).WithMany(p => p.Appointments).HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.SetNull);

            // Payment - Appointment 1:1 (optional)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Appointment)
                .WithOne(a => a.Payment)
                .HasForeignKey<Payment>(p => p.AppointmentId);

            // Report relations
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Patient).WithMany(p => p.Reports).HasForeignKey(r => r.PatientId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.Doctor).WithMany(d => d.Reports).HasForeignKey(r => r.DoctorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Prescription -> Report
            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Report).WithMany(r => r.Prescriptions)
                .HasForeignKey(p => p.ReportId).OnDelete(DeleteBehavior.Cascade);

            // Department - Doctor
            modelBuilder.Entity<Department>()
                .HasMany(d => d.Doctors).WithOne().HasForeignKey("DepartmentId").OnDelete(DeleteBehavior.SetNull);
        }
    }
}
