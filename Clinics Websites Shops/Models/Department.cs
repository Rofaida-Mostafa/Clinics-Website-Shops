namespace Clinics_Websites_Shops.Models
{
    public class Department : IMustHaveTenant
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // Default name (fallback)
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
        public string TenantId { get; set; }
        //Status
        public bool Status { get; set; } = true;
        public string Description { get; set; } = null!;
        public string MainImg { get; set; } = null!;
        // Multi-language support
        public ICollection<DepartmentTranslation> Translations { get; set; } = new List<DepartmentTranslation>();
    }
}
