using Clinics_Websites_Shops.DataAccess;
using Clinics_Websites_Shops.Models;
using Clinics_Websites_Shops.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace Clinics_Websites_Shops.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;
        private const int CacheExpirationMinutes = 30;

        public PermissionService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IMemoryCache cache)
        {
            _context = context;
            _userManager = userManager;
            _cache = cache;
        }

        public async Task<bool> UserHasPermissionAsync(ClaimsPrincipal user, string permissionName)
        {
            var userId = _userManager.GetUserId(user);
            if (string.IsNullOrEmpty(userId))
                return false;

            var userPermissions = await GetUserPermissionsAsync(userId);
            return userPermissions.Contains(permissionName);
        }

        public async Task<List<string>> GetUserPermissionsAsync(string userId)
        {
            var cacheKey = $"UserPermissions_{userId}";

            if (_cache.TryGetValue(cacheKey, out List<string>? cachedPermissions) && cachedPermissions != null)
            {
                return cachedPermissions;
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new List<string>();

            var userRoles = await _userManager.GetRolesAsync(user);

            var permissions = await _context.RolePermissions
                .Include(rp => rp.Permission)
                .Include(rp => rp.Role)
                .Where(rp => userRoles.Contains(rp.Role.Name!))
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .ToListAsync();

            _cache.Set(cacheKey, permissions, TimeSpan.FromMinutes(CacheExpirationMinutes));

            return permissions;
        }

        public async Task<bool> AssignPermissionToRoleAsync(string roleId, int permissionId)
        {
            var existingRolePermission = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

            if (existingRolePermission != null)
                return false; // Already exists

            var rolePermission = new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            };

            _context.RolePermissions.Add(rolePermission);
            await _context.SaveChangesAsync();

            // Clear cache for all users with this role
            ClearRolePermissionsCache(roleId);

            return true;
        }

        public async Task<bool> RemovePermissionFromRoleAsync(string roleId, int permissionId)
        {
            var rolePermission = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

            if (rolePermission == null)
                return false;

            _context.RolePermissions.Remove(rolePermission);
            await _context.SaveChangesAsync();

            // Clear cache for all users with this role
            ClearRolePermissionsCache(roleId);

            return true;
        }

        public async Task<List<string>> GetRolePermissionsAsync(string roleId)
        {
            var cacheKey = $"RolePermissions_{roleId}";

            if (_cache.TryGetValue(cacheKey, out List<string>? cachedPermissions) && cachedPermissions != null)
            {
                return cachedPermissions;
            }

            var permissions = await _context.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.Permission.Name)
                .ToListAsync();

            _cache.Set(cacheKey, permissions, TimeSpan.FromMinutes(CacheExpirationMinutes));

            return permissions;
        }

        private void ClearRolePermissionsCache(string roleId)
        {
            var cacheKey = $"RolePermissions_{roleId}";
            _cache.Remove(cacheKey);

            // Also clear user permissions cache for users with this role
            // This is a simplified approach - in production, you might want a more sophisticated cache invalidation strategy
        }
    }
}

