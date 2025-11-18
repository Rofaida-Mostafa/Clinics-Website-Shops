using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Clinics_Websites_Shops.Areas.Admin.ViewModel
{
    public class CreateNurseViewModel : IValidatableObject
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
        public string Description { get; set; } = "Nurse";
        
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
        
        // Nurse fields
        [Required, StringLength(50)]
        [Display(Name = "Nurse ID")]
        public string NurseId { get; set; } = null!;
        
        [Required]
        [Display(Name = "Salary")]
        [Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive number")]
        public decimal Salary { get; set; }
        
        // For dropdowns
        public List<SelectListItem> Departments { get; set; } = new();
        
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Salary < 0)
            {
                yield return new ValidationResult(
                    "Salary must be a positive number",
                    new[] { nameof(Salary) }
                );
            }
            
            if (string.IsNullOrWhiteSpace(NurseId))
            {
                yield return new ValidationResult(
                    "Nurse ID is required",
                    new[] { nameof(NurseId) }
                );
            }
        }
    }
    
    public class EditNurseViewModel : CreateNurseViewModel
    {
        // ApplicationUser fields for editing (read-only display)
        [Display(Name = "Application User")]
        public string ApplicationUserId { get; set; } = null!;

        // For dropdowns (needed for Edit)
        public List<SelectListItem> ApplicationUsers { get; set; } = new();
    }

    public class NurseListViewModel
    {
        public string NurseId { get; set; } = null!;
        public decimal Salary { get; set; }
        public string? ApplicationUserName { get; set; }
        public string? ApplicationUserEmail { get; set; }
        public string? PhoneNumber { get; set; }
        public int Rate { get; set; }
        public string? Description { get; set; }

        // Original nurse for reference
        public Nurse OriginalNurse { get; set; } = null!;
    }
}

