using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Clinics_Websites_Shops.Models;

namespace Clinics_Websites_Shops.Areas.Admin.ViewModel
{
    public class CreateDoctorViewModel : IValidatableObject
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
        public string Description { get; set; } = "Doctor";
        
        [Range(1, 10)]
        [Display(Name = "Rate")]
        public int Rate { get; set; } = 5;
        
        [Required, StringLength(6, MinimumLength = 6)]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
        
        [Required, Compare("Password")]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = null!;
        
        // Doctor fields
        [Required, StringLength(50)]
        [Display(Name = "Doctor ID")]
        public string DoctorId { get; set; } = null!;
        
        [Required, StringLength(20)]
        [Display(Name = "License Number")]
        public string LicenseNumber { get; set; } = null!;
        
        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }
        
        [Required, StringLength(200)]
        [Display(Name = "Medical Degree")]
        public string MedicalDegree { get; set; } = null!;
        
        [StringLength(500)]
        [Display(Name = "Additional Certifications")]
        public string? AdditionalCertifications { get; set; }
        
        [Range(0, 60)]
        [Display(Name = "Years of Experience")]
        public int YearsOfExperience { get; set; }
        
        [StringLength(200)]
        [Display(Name = "Medical School")]
        public string? MedicalSchool { get; set; }
        
        [Display(Name = "Graduation Date")]
        public DateTime? GraduationDate { get; set; }
        
        [Display(Name = "Doctor Rank")]
        public DoctorRank Rank { get; set; } = DoctorRank.Resident;
        
        [Display(Name = "Employment Status")]
        public EmploymentStatus EmploymentStatus { get; set; } = EmploymentStatus.FullTime;
        
        [Display(Name = "Joined Date")]
        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Base Salary")]
        public decimal BaseSalary { get; set; }
        
        [Display(Name = "Consultation Fee")]
        public decimal? ConsultationFee { get; set; }
        
        [Display(Name = "Hourly Rate")]
        public decimal? HourlyRate { get; set; }
        
        [Display(Name = "Working Hours Start")]
        public TimeSpan? WorkingHoursStart { get; set; }
        
        [Display(Name = "Working Hours End")]
        public TimeSpan? WorkingHoursEnd { get; set; }
        
        [StringLength(100)]
        [Display(Name = "Working Days")]
        public string? WorkingDays { get; set; }
        
        [StringLength(1000)]
        [Display(Name = "Biography")]
        public string? Biography { get; set; }
        
        [StringLength(500)]
        [Display(Name = "Languages")]
        public string? Languages { get; set; }
        
        [StringLength(500)]
        [Display(Name = "Awards")]
        public string? Awards { get; set; }
        
        [StringLength(20)]
        [Display(Name = "Direct Phone")]
        public string? DirectPhone { get; set; }
        
        [StringLength(100)]
        [Display(Name = "Office Location")]
        public string? OfficeLocation { get; set; }
        
        [StringLength(20)]
        [Display(Name = "Room Number")]
        public string? RoomNumber { get; set; }
        
        [StringLength(100)]
        [Display(Name = "Emergency Contact Name")]
        public string? EmergencyContactName { get; set; }
        
        [StringLength(20)]
        [Display(Name = "Emergency Contact Phone")]
        public string? EmergencyContactPhone { get; set; }
        
        [StringLength(50)]
        [Display(Name = "Emergency Contact Relation")]
        public string? EmergencyContactRelation { get; set; }
        
        [StringLength(200)]
        [Display(Name = "Profile Image URL")]
        public string? ProfileImageUrl { get; set; }
        
        [Display(Name = "Available for Emergency")]
        public bool IsAvailableForEmergency { get; set; } = true;
        
        [Display(Name = "Accepting New Patients")]
        public bool IsAcceptingNewPatients { get; set; } = true;
        
        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
        
        // For dropdowns
        public List<SelectListItem> Departments { get; set; } = new();
        
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (WorkingHoursStart.HasValue && WorkingHoursEnd.HasValue)
            {
                if (WorkingHoursStart >= WorkingHoursEnd)
                {
                    yield return new ValidationResult(
                        "Working hours start must be before working hours end.",
                        new[] { nameof(WorkingHoursStart), nameof(WorkingHoursEnd) }
                    );
                }
            }
            
            if (GraduationDate.HasValue && GraduationDate > DateTime.Now)
            {
                yield return new ValidationResult(
                    "Graduation date cannot be in the future.",
                    new[] { nameof(GraduationDate) }
                );
            }
        }
    }
    
    public class EditDoctorViewModel : CreateDoctorViewModel
    {
        public int Id { get; set; }
        
        // ApplicationUser fields for editing (read-only display)
        [Display(Name = "Application User")]
        public string ApplicationUserId { get; set; } = null!;
        
        // For dropdowns (needed for Edit)
        public List<SelectListItem> ApplicationUsers { get; set; } = new();
    }
    
    public class DoctorListViewModel
    {
        public int Id { get; set; }
        public string DoctorId { get; set; } = null!;
        public string LicenseNumber { get; set; } = null!;
        public string MedicalDegree { get; set; } = null!;
        public int YearsOfExperience { get; set; }
        public DoctorRank Rank { get; set; }
        public EmploymentStatus EmploymentStatus { get; set; }
        public decimal BaseSalary { get; set; }
        public bool IsActive { get; set; }
        public string? DepartmentName { get; set; }
        public string? ApplicationUserName { get; set; }
        public string? ApplicationUserEmail { get; set; }
        public string? ProfileImageUrl { get; set; }
        public DateTime JoinedDate { get; set; }
        
        // Original doctor for reference
        public Doctor OriginalDoctor { get; set; } = null!;
    }
}