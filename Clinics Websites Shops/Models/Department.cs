namespace Clinics_Websites_Shops.Models
{
    public class Department : IMustHaveTenant
    {
        public int Id { get; set; } 
        
        // Navigation Properties
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
        
        public string TenantId { get; set; } = null!;
        
        //Status
        public bool Status { get; set; } = true;
        public string? MainImg { get; set; }
        
        // Multi-language support
        public ICollection<DepartmentTranslation> Translations { get; set; } = new List<DepartmentTranslation>();
    }
}
