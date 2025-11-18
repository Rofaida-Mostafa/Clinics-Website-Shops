using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Clinics_Websites_Shops.Models
{
    public class Nurse : IMustHaveTenant
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string NurseId { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Salary { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = null!;
        public ApplicationUser? ApplicationUser { get; set; }

        [Required]
        public string TenantId { get; set; } = null!;
    }
}
