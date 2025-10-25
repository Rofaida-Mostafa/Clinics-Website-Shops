using Microsoft.AspNetCore.Identity;

namespace Clinics_Websites_Shops.Models
{
    public class Person : IdentityUser, ITenant
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int Rate { get; set; }
        public string TenantId { get; set; } = null!;
    }
}
