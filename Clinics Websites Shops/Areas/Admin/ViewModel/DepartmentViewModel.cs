using System.ComponentModel.DataAnnotations;

namespace Clinics_Websites_Shops.Areas.Admin.ViewModel
{
    public class DepartmentTranslationViewModel
    {
        public string LanguageCode { get; set; } = null!;
        public string LanguageName { get; set; } = null!;
        
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = null!;
        
        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = null!;
    }

    public class CreateDepartmentViewModel
    {
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = null!; // Default name (fallback)
        
        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = null!; // Default description
        
        public string MainImg { get; set; } = null!;
        
        public bool Status { get; set; } = true;
        
        // Dynamic translations for all supported languages
        public List<DepartmentTranslationViewModel> Translations { get; set; } = new();
    }

    public class EditDepartmentViewModel
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = null!; // Default name (fallback)
        
        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = null!; // Default description
        
        public string MainImg { get; set; } = null!;
        
        public bool Status { get; set; } = true;
        
        // Dynamic translations for all supported languages
        public List<DepartmentTranslationViewModel> Translations { get; set; } = new();
    }

    public class DepartmentListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // Localized based on current culture
        public string Description { get; set; } = null!; // Localized based on current culture
        public string MainImg { get; set; } = null!;
        public bool Status { get; set; }
        public int DoctorsCount { get; set; }
        
        // Original department for reference
        public Department OriginalDepartment { get; set; } = null!;
    }
}