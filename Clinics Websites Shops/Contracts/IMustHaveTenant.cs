using System.ComponentModel.DataAnnotations;

namespace Clinics_Websites_Shops.Contracts
{
    public interface IMustHaveTenant
    {
        [Required]
        public string TenantId { get; set; }
    }
}
