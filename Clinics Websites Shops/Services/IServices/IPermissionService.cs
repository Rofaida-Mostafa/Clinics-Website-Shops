using System.Security.Claims;

namespace Clinics_Websites_Shops.Services.IServices
{
    public interface IPermissionService
    {
        Task<bool> UserHasPermissionAsync(ClaimsPrincipal user, string permissionName);
        Task<List<string>> GetUserPermissionsAsync(string userId);
        Task<bool> AssignPermissionToRoleAsync(string roleId, int permissionId);
        Task<bool> RemovePermissionFromRoleAsync(string roleId, int permissionId);
        Task<List<string>> GetRolePermissionsAsync(string roleId);
    }
}

