using System.ComponentModel.DataAnnotations;

namespace Clinics_Websites_Shops.Models
{
    public class Appointment : IMustHaveTenant
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan? Time { get; set; }
        public string? Status { get; set; }
        public string? DoctorId { get; set; }
        public Doctor? Doctor { get; set; }
        public string? PatientId { get; set; }
        public Patient? Patient { get; set; }
        public Payment? Payment { get; set; }
        
        [Required]
        public string TenantId { get; set; } = null!;
    }
}
