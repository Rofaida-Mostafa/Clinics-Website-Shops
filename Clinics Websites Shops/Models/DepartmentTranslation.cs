namespace Clinics_Websites_Shops.Models
{
    public class DepartmentTranslation
    {
        public int Id { get; set; }
        public int DepartmentId { get; set; }
        public string LanguageCode { get; set; } = null!; // e.g., "en-US", "ar-EG", "de-DE"
        public string Name { get; set; } = null!;
        
        public string Description { get; set; } = null!;
        
        // Navigation property
        public Department Department { get; set; } = null!;
    }
}

