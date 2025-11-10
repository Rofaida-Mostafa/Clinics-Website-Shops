using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Clinics_Websites_Shops.Models
{
    public class Doctor : IMustHaveTenant
    {
        public int Id { get; set; }
        
        // Professional Identity
        [Required, StringLength(50)]
        public string DoctorId { get; set; } = null!; // Unique doctor identifier (e.g., "DOC001")
        
        [Required, StringLength(20)]
        public string LicenseNumber { get; set; } = null!; // Medical license number
        
        [Required, StringLength(200)]
        public string MedicalDegree { get; set; } = null!; // MBBS, MD, etc.
        
        [StringLength(500)]
        public string? AdditionalCertifications { get; set; } // Board certifications, fellowships
        
        // Experience & Career
        [Range(0, 60)]
        public int YearsOfExperience { get; set; }
        
        [StringLength(200)]
        public string? MedicalSchool { get; set; }
        
        public DateTime? GraduationDate { get; set; }
        
        // Professional Status
        public DoctorRank Rank { get; set; } = DoctorRank.Resident;
        
        public EmploymentStatus EmploymentStatus { get; set; } = EmploymentStatus.FullTime;
        
        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
        
        // Financial Information
        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseSalary { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ConsultationFee { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? HourlyRate { get; set; }
        
        // Work Schedule
        public TimeSpan? WorkingHoursStart { get; set; }
        
        public TimeSpan? WorkingHoursEnd { get; set; }
        
        [StringLength(100)]
        public string? WorkingDays { get; set; } // e.g., "Monday,Tuesday,Wednesday"
        
        // Professional Information
        [StringLength(1000)]
        public string? Biography { get; set; }
        
        [StringLength(500)]
        public string? Languages { get; set; } // Languages spoken
        
        [StringLength(500)]
        public string? Awards { get; set; }
        
        // Contact & Location
        [StringLength(20)]
        public string? DirectPhone { get; set; }
        
        [StringLength(100)]
        public string? OfficeLocation { get; set; }
        
        [StringLength(20)]
        public string? RoomNumber { get; set; }
        
        // Emergency Contact
        [StringLength(100)]
        public string? EmergencyContactName { get; set; }
        
        [StringLength(20)]
        public string? EmergencyContactPhone { get; set; }
        
        [StringLength(50)]
        public string? EmergencyContactRelation { get; set; }
        
        // Professional Profile
        [StringLength(200)]
        public string? ProfileImageUrl { get; set; }
        
        public bool IsAvailableForEmergency { get; set; } = true;
        
        public bool IsAcceptingNewPatients { get; set; } = true;
        
        public bool IsActive { get; set; } = true;
        
        // System Fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastModified { get; set; }
        
        [StringLength(100)]
        public string? CreatedBy { get; set; }
        
        [StringLength(100)]
        public string? ModifiedBy { get; set; }
        
        // Navigation Properties
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Report> Reports { get; set; } = new List<Report>();
        
        // User Account Reference
        [Required]
        public string ApplicationUserId { get; set; } = null!;
        public ApplicationUser? ApplicationUser { get; set; }
        
        // Tenant & Department
        [Required]
        public string TenantId { get; set; } = null!;
        
        public int? DepartmentId { get; set; } // Made nullable in case doctor works across departments
        public Department? Department { get; set; }
    }
    
    public enum DoctorRank
    {
        Intern = 1,
        Resident = 2,
        Fellow = 3,
        Attending = 4,
        ChiefResident = 5,
        AssistantProfessor = 6,
        AssociateProfessor = 7,
        Professor = 8,
        ChiefOfService = 9,
        MedicalDirector = 10
    }
    
    public enum EmploymentStatus
    {
        FullTime = 1,
        PartTime = 2,
        Consultant = 3,
        Locum = 4,
        Retired = 5,
        OnLeave = 6
    }
}
