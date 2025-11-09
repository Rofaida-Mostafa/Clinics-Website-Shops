using System.ComponentModel.DataAnnotations;

namespace Clinics_Websites_Shops.Areas.Admin.ViewModel
{
    public class DepartmentTranslationViewModel
    {
        public string LanguageCode { get; set; } = null!;
        public string LanguageName { get; set; } = null!;
        
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
    }

    public class CreateDepartmentViewModel : IValidatableObject
    {
        public string? MainImg { get; set; }
        
        public bool Status { get; set; } = true;
        
        // Dynamic translations for all supported languages
        public List<DepartmentTranslationViewModel> Translations { get; set; } = new();
        
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // At least one translation must have a name
            if (!Translations.Any(t => !string.IsNullOrWhiteSpace(t.Name)))
            {
                yield return new ValidationResult(
                    "At least one language translation must have a name.",
                    new[] { nameof(Translations) }
                );
            }
        }
    }

    public class EditDepartmentViewModel : IValidatableObject
    {
        public int Id { get; set; }
        
        public string? MainImg { get; set; }
        
        public bool Status { get; set; } = true;
        
        // Dynamic translations for all supported languages
        public List<DepartmentTranslationViewModel> Translations { get; set; } = new();
        
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // At least one translation must have a name
            if (!Translations.Any(t => !string.IsNullOrWhiteSpace(t.Name)))
            {
                yield return new ValidationResult(
                    "At least one language translation must have a name.",
                    new[] { nameof(Translations) }
                );
            }
        }
    }

    public class DepartmentListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // Localized based on current culture
        public string Description { get; set; } = null!; // Localized based on current culture
        public string? MainImg { get; set; }
        public bool Status { get; set; }
        public int DoctorsCount { get; set; }
        
        // Original department for reference
        public Department OriginalDepartment { get; set; } = null!;
    }
}