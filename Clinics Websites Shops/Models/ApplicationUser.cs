using Microsoft.AspNetCore.Identity;

namespace Clinics_Websites_Shops.Models
{
    public class ApplicationUser : IdentityUser, IMustHaveTenant

    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int Rate { get; set; }
        public string TenantId { get; set; } = null!;
    }
}
