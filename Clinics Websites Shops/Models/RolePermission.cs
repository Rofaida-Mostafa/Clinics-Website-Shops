namespace Clinics_Websites_Shops.Models
{
    public class RolePermission
    {
        public int Id { get; set; }

        public string RoleId { get; set; } = null!;
        public ApplicationRole Role { get; set; } = null!;

        public int PermissionId { get; set; }
        public Permission Permission { get; set; } = null!;

        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
        public string? GrantedBy { get; set; }
    }
}

