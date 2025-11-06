namespace Clinics_Websites_Shops.Models
{
    public class Prescription : IMustHaveTenant
    {
        public int Id { get; set; }
        public DateTime PrescriptionDate { get; set; } = DateTime.UtcNow;
        public string Details { get; set; } = null!;
        public int ReportId { get; set; }
        public Report Report { get; set; } = null!;
       public string TenantId { get; set; }

    }
}
