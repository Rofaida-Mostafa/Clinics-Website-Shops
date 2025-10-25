using System.ComponentModel.DataAnnotations;

namespace Clinics_Websites_Shops.Settings
{
    public class Tenant
    {
        [Key]  
        public string TId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string ConnectionString { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Domain { get; set; } = string.Empty;

        public bool Status { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
