using System.ComponentModel.DataAnnotations;

namespace Clinics_Websites_Shops.Areas.Admin.ViewModel
{
    public class CreateRoleViewModel
    {
        [Required, StringLength(100)]
        [Display(Name = "Role Name")]
        public string Name { get; set; } = null!;

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        public List<PermissionGroupViewModel> PermissionGroups { get; set; } = new();
        public List<int> SelectedPermissionIds { get; set; } = new();
    }

    public class EditRoleViewModel : CreateRoleViewModel
    {
        public string Id { get; set; } = null!;
        public bool IsSystemRole { get; set; }
    }

    public class RoleListViewModel
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsSystemRole { get; set; }
        public int UserCount { get; set; }
        public int PermissionCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RolePermissionsViewModel
    {
        public string RoleId { get; set; } = null!;
        public string RoleName { get; set; } = null!;
        public bool IsSystemRole { get; set; }
        public List<PermissionGroupViewModel> PermissionGroups { get; set; } = new();
    }

    public class PermissionGroupViewModel
    {
        public string Module { get; set; } = null!;
        public List<PermissionItemViewModel> Permissions { get; set; } = new();
    }

    public class PermissionItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string? Description { get; set; }
        public string Action { get; set; } = null!;
        public bool IsAssigned { get; set; }
    }
}

