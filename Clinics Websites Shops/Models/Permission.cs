using System.ComponentModel.DataAnnotations;

namespace Clinics_Websites_Shops.Models
{
    public class Permission
    {
        public int Id { get; set; }
        
        [Required, StringLength(100)]
        public string Name { get; set; } = null!; // e.g., "Doctors.View", "Patients.Create"
        
        [Required, StringLength(100)]
        public string DisplayName { get; set; } = null!; // e.g., "View Doctors"
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Required, StringLength(50)]
        public string Module { get; set; } = null!; // e.g., "Doctors", "Patients", "Appointments"
        
        [Required, StringLength(50)]
        public string Action { get; set; } = null!; // e.g., "View", "Create", "Edit", "Delete"
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}

