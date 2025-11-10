using System.ComponentModel.DataAnnotations;

namespace Clinics_Websites_Shops.Models
{
    public class Report : IMustHaveTenant
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? BloodType { get; set; }
        public string? Notes { get; set; }
        public string? PatientId { get; set; }
        public Patient? Patient { get; set; }
        public string? DoctorId { get; set; }
        public Doctor? Doctor { get; set; }
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>(); 
        
        [Required]
        public string TenantId { get; set; } = null!;
    }
}
