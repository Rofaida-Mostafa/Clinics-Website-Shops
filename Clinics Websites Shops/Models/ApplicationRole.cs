using Microsoft.AspNetCore.Identity;

namespace Clinics_Websites_Shops.Models
{
    public class ApplicationRole : IdentityRole
    {
        public string? Description { get; set; }
        public bool IsSystemRole { get; set; } // Cannot be deleted
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastModified { get; set; }
        
        // Navigation property
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}

