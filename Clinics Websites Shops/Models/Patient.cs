using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Clinics_Websites_Shops.Models
{
    public class Patient : IMustHaveTenant
    {
        public int Id { get; set; }

        // Patient Identity
        [Required, StringLength(50)]
        public string PatientId { get; set; } = null!; // Unique patient identifier (e.g., "PAT001")

        // Personal Information
        [StringLength(20)]
        public string? NationalId { get; set; } // National ID or SSN

        [StringLength(20)]
        public string? PassportNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [Range(0, 150)]
        public int? Age { get; set; }

        public Gender Gender { get; set; } = Gender.NotSpecified;

        [StringLength(50)]
        public string? Nationality { get; set; }

        [StringLength(100)]
        public string? Occupation { get; set; }

        public MaritalStatus MaritalStatus { get; set; } = MaritalStatus.Single;

        // Contact Information
        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(20)]
        public string? AlternatePhone { get; set; }

        // Emergency Contact
        [StringLength(100)]
        public string? EmergencyContactName { get; set; }

        [StringLength(20)]
        public string? EmergencyContactPhone { get; set; }

        [StringLength(50)]
        public string? EmergencyContactRelation { get; set; }

        [StringLength(200)]
        public string? EmergencyContactAddress { get; set; }

        // Medical Information
        [StringLength(20)]
        public string? BloodType { get; set; } // A+, B+, O-, etc.

        [Column(TypeName = "decimal(5,2)")]
        public decimal? Height { get; set; } // in cm

        [Column(TypeName = "decimal(5,2)")]
        public decimal? Weight { get; set; } // in kg

        [StringLength(1000)]
        public string? Allergies { get; set; } // Comma-separated list

        [StringLength(1000)]
        public string? ChronicDiseases { get; set; } // Comma-separated list

        [StringLength(1000)]
        public string? CurrentMedications { get; set; } // Comma-separated list

        [StringLength(1000)]
        public string? PreviousSurgeries { get; set; } // Comma-separated list

        [StringLength(500)]
        public string? FamilyMedicalHistory { get; set; }

        public bool IsSmoker { get; set; } = false;

        public bool IsAlcoholConsumer { get; set; } = false;

        [StringLength(500)]
        public string? SpecialNotes { get; set; }

        // Insurance Information
        [StringLength(100)]
        public string? InsuranceProvider { get; set; }

        [StringLength(50)]
        public string? InsurancePolicyNumber { get; set; }

        public DateTime? InsuranceExpiryDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? InsuranceCoverageAmount { get; set; }

        // Registration & Status
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        public DateTime? LastVisitDate { get; set; }

        public PatientStatus Status { get; set; } = PatientStatus.Active;

        public bool IsActive { get; set; } = true;

        [StringLength(200)]
        public string? ProfileImageUrl { get; set; }

        // Preferences
        [StringLength(100)]
        public string? PreferredLanguage { get; set; }

        [StringLength(100)]
        public string? PreferredContactMethod { get; set; } // Phone, Email, SMS

        public bool AllowSmsNotifications { get; set; } = true;

        public bool AllowEmailNotifications { get; set; } = true;

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

        // Tenant
        [Required]
        public string TenantId { get; set; } = null!;
    }

    public enum Gender
    {
        NotSpecified = 0,
        Male = 1,
        Female = 2,
        Other = 3
    }

    public enum MaritalStatus
    {
        Single = 1,
        Married = 2,
        Divorced = 3,
        Widowed = 4,
        Separated = 5
    }

    public enum PatientStatus
    {
        Active = 1,
        Inactive = 2,
        Discharged = 3,
        Deceased = 4,
        Transferred = 5
    }
}