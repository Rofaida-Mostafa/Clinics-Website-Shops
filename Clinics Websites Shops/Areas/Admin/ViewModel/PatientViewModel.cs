using System.ComponentModel.DataAnnotations;
using Clinics_Websites_Shops.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Clinics_Websites_Shops.Areas.Admin.ViewModel
{
    public class CreatePatientViewModel : IValidatableObject
    {
        // ApplicationUser fields
        [Required, StringLength(100)]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = null!;
        
        [Required, EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;
        
        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
        
        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; } = "Patient";
        
        [Range(1, 10)]
        [Display(Name = "Rate")]
        public int Rate { get; set; } = 5;
        
        [Required, StringLength(6, MinimumLength = 6)]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
        
        // Patient Identity
        [Required, StringLength(50)]
        [Display(Name = "Patient ID")]
        public string PatientId { get; set; } = null!;
        
        // Personal Information
        [StringLength(20)]
        [Display(Name = "National ID")]
        public string? NationalId { get; set; }
        
        [StringLength(20)]
        [Display(Name = "Passport Number")]
        public string? PassportNumber { get; set; }
        
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
        
        [Range(0, 150)]
        [Display(Name = "Age")]
        public int? Age { get; set; }
        
        [Display(Name = "Gender")]
        public Gender Gender { get; set; } = Gender.NotSpecified;
        
        [StringLength(50)]
        [Display(Name = "Nationality")]
        public string? Nationality { get; set; }
        
        [StringLength(100)]
        [Display(Name = "Occupation")]
        public string? Occupation { get; set; }
        
        [Display(Name = "Marital Status")]
        public MaritalStatus MaritalStatus { get; set; } = MaritalStatus.Single;
        
        // Contact Information
        [StringLength(200)]
        [Display(Name = "Address")]
        public string? Address { get; set; }
        
        [StringLength(100)]
        [Display(Name = "City")]
        public string? City { get; set; }
        
        [StringLength(100)]
        [Display(Name = "State")]
        public string? State { get; set; }
        
        [StringLength(20)]
        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }
        
        [StringLength(100)]
        [Display(Name = "Country")]
        public string? Country { get; set; }
        
        [Phone]
        [StringLength(20)]
        [Display(Name = "Alternate Phone")]
        public string? AlternatePhone { get; set; }
        
        // Emergency Contact
        [StringLength(100)]
        [Display(Name = "Emergency Contact Name")]
        public string? EmergencyContactName { get; set; }
        
        [Phone]
        [StringLength(20)]
        [Display(Name = "Emergency Contact Phone")]
        public string? EmergencyContactPhone { get; set; }
        
        [StringLength(50)]
        [Display(Name = "Emergency Contact Relation")]
        public string? EmergencyContactRelation { get; set; }
        
        [StringLength(200)]
        [Display(Name = "Emergency Contact Address")]
        public string? EmergencyContactAddress { get; set; }
        
        // Medical Information
        [StringLength(20)]
        [Display(Name = "Blood Type")]
        public string? BloodType { get; set; }
        
        [Range(0, 300)]
        [Display(Name = "Height (cm)")]
        public decimal? Height { get; set; }
        
        [Range(0, 500)]
        [Display(Name = "Weight (kg)")]
        public decimal? Weight { get; set; }
        
        [StringLength(1000)]
        [Display(Name = "Allergies")]
        public string? Allergies { get; set; }
        
        [StringLength(1000)]
        [Display(Name = "Chronic Diseases")]
        public string? ChronicDiseases { get; set; }
        
        [StringLength(1000)]
        [Display(Name = "Current Medications")]
        public string? CurrentMedications { get; set; }
        
        [StringLength(1000)]
        [Display(Name = "Previous Surgeries")]
        public string? PreviousSurgeries { get; set; }
        
        [StringLength(500)]
        [Display(Name = "Family Medical History")]
        public string? FamilyMedicalHistory { get; set; }
        
        [Display(Name = "Smoker")]
        public bool IsSmoker { get; set; } = false;
        
        [Display(Name = "Alcohol Consumer")]
        public bool IsAlcoholConsumer { get; set; } = false;
        
        [StringLength(500)]
        [Display(Name = "Special Notes")]
        public string? SpecialNotes { get; set; }
        
        // Insurance Information
        [StringLength(100)]
        [Display(Name = "Insurance Provider")]
        public string? InsuranceProvider { get; set; }
        
        [StringLength(50)]
        [Display(Name = "Insurance Policy Number")]
        public string? InsurancePolicyNumber { get; set; }
        
        [Display(Name = "Insurance Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime? InsuranceExpiryDate { get; set; }
        
        [Range(0, 10000000)]
        [Display(Name = "Insurance Coverage Amount")]
        public decimal? InsuranceCoverageAmount { get; set; }
        
        // Registration & Status
        [Display(Name = "Registration Date")]
        [DataType(DataType.Date)]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Status")]
        public PatientStatus Status { get; set; } = PatientStatus.Active;
        
        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
        
        [StringLength(200)]
        [Display(Name = "Profile Image URL")]
        public string? ProfileImageUrl { get; set; }
        
        // Preferences
        [StringLength(100)]
        [Display(Name = "Preferred Language")]
        public string? PreferredLanguage { get; set; }
        
        [StringLength(100)]
        [Display(Name = "Preferred Contact Method")]
        public string? PreferredContactMethod { get; set; }
        
        [Display(Name = "Allow SMS Notifications")]
        public bool AllowSmsNotifications { get; set; } = true;
        
        [Display(Name = "Allow Email Notifications")]
        public bool AllowEmailNotifications { get; set; } = true;
        
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DateOfBirth.HasValue && DateOfBirth.Value > DateTime.Now)
            {
                yield return new ValidationResult(
                    "Date of Birth cannot be in the future",
                    new[] { nameof(DateOfBirth) }
                );
            }
            
            if (InsuranceExpiryDate.HasValue && InsuranceExpiryDate.Value < DateTime.Now)
            {
                yield return new ValidationResult(
                    "Insurance has expired",
                    new[] { nameof(InsuranceExpiryDate) }
                );
            }
            
            if (Height.HasValue && (Height.Value < 0 || Height.Value > 300))
            {
                yield return new ValidationResult(
                    "Height must be between 0 and 300 cm",
                    new[] { nameof(Height) }
                );
            }
            
            if (Weight.HasValue && (Weight.Value < 0 || Weight.Value > 500))
            {
                yield return new ValidationResult(
                    "Weight must be between 0 and 500 kg",
                    new[] { nameof(Weight) }
                );
            }
        }
    }
    
    public class EditPatientViewModel : CreatePatientViewModel
    {
        public int Id { get; set; }
        
        // ApplicationUser fields for editing (read-only display)
        [Display(Name = "Application User")]
        public string ApplicationUserId { get; set; } = null!;
        
        // For dropdowns (needed for Edit)
        public List<SelectListItem> ApplicationUsers { get; set; } = new();
    }
    
    public class PatientListViewModel
    {
        public int Id { get; set; }
        public string PatientId { get; set; } = null!;
        public string? NationalId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? Age { get; set; }
        public Gender Gender { get; set; }
        public string? BloodType { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? LastVisitDate { get; set; }
        public PatientStatus Status { get; set; }
        public bool IsActive { get; set; }
        public string? ApplicationUserName { get; set; }
        public string? ApplicationUserEmail { get; set; }
        public string? ApplicationUserPhone { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? InsuranceProvider { get; set; }
        
        // Original patient for reference
        public Patient OriginalPatient { get; set; } = null!;
    }
}

