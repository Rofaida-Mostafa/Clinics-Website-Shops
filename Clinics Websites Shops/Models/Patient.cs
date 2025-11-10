using System.ComponentModel.DataAnnotations;

namespace Clinics_Websites_Shops.Models
{
    public class Patient : IMustHaveTenant
    {
        [Key, StringLength(50)]
        public string PatientId { get; set; } = null!; // Keep as primary key
        
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Report> Reports { get; set; } = new List<Report>();
        
        [Required]
        public string ApplicationUserId { get; set; } = null!;
        public ApplicationUser? ApplicationUser { get; set; }
        
        [Required]
        public string TenantId { get; set; } = null!;
    }
}