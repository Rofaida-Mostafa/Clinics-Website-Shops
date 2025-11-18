using Clinics_Websites_Shops.Areas.Admin.ViewModel;
using Clinics_Websites_Shops.Attributes;
using Clinics_Websites_Shops.DataAccess;
using Clinics_Websites_Shops.Models;
using Clinics_Websites_Shops.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Clinics_Websites_Shops.Areas.Admin.Controllers
{
    [Area("Admin")]
  //  [Authorize]
    public class RoleController : Controller
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IPermissionService _permissionService;
        private readonly IStringLocalizer<RoleController> _localizer;

        public RoleController(
            RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IPermissionService permissionService,
            IStringLocalizer<RoleController> localizer)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
            _permissionService = permissionService;
            _localizer = localizer;
        }

     [Permission("Roles.View")]
        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var roleViewModels = new List<RoleListViewModel>();

            foreach (var role in roles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
                var permissionCount = await _context.RolePermissions
                    .CountAsync(rp => rp.RoleId == role.Id);

                roleViewModels.Add(new RoleListViewModel
                {
                    Id = role.Id,
                    Name = role.Name!,
                    Description = role.Description,
                    IsSystemRole = role.IsSystemRole,
                    UserCount = usersInRole.Count,
                    PermissionCount = permissionCount,
                    CreatedAt = role.CreatedAt
                });
            }

            return View(roleViewModels);
        }

        [HttpGet]
        [Permission("Roles.Create")]
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateRoleViewModel
            {
                PermissionGroups = await GetPermissionGroupsAsync()
            };
            return View(viewModel);
        }

        [HttpPost]
        [Permission("Roles.Create")]
        public async Task<IActionResult> Create(CreateRoleViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.PermissionGroups = await GetPermissionGroupsAsync();
                return View(viewModel);
            }

            var roleExists = await _roleManager.RoleExistsAsync(viewModel.Name);
            if (roleExists)
            {
                ModelState.AddModelError("Name", "Role already exists");
                viewModel.PermissionGroups = await GetPermissionGroupsAsync();
                return View(viewModel);
            }

            var role = new ApplicationRole
            {
                Name = viewModel.Name,
                Description = viewModel.Description,
                IsSystemRole = false
            };

            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                // Assign selected permissions to the role
                if (viewModel.SelectedPermissionIds != null && viewModel.SelectedPermissionIds.Any())
                {
                    var rolePermissions = viewModel.SelectedPermissionIds.Select(permissionId => new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permissionId
                    }).ToList();

                    await _context.RolePermissions.AddRangeAsync(rolePermissions);
                    await _context.SaveChangesAsync();
                }

                TempData["success-notification"] = _localizer["addRoleSuccess"].Value;
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            viewModel.PermissionGroups = await GetPermissionGroupsAsync();
            return View(viewModel);
        }

        [HttpGet]
        [Permission("Roles.Edit")]
        public async Task<IActionResult> Edit(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                TempData["error-notification"] = _localizer["roleNotFound"].Value;
                return RedirectToAction(nameof(Index));
            }

            var rolePermissionIds = await _context.RolePermissions
                .Where(rp => rp.RoleId == id)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            var viewModel = new EditRoleViewModel
            {
                Id = role.Id,
                Name = role.Name!,
                Description = role.Description,
                IsSystemRole = role.IsSystemRole,
                PermissionGroups = await GetPermissionGroupsAsync(rolePermissionIds),
                SelectedPermissionIds = rolePermissionIds
            };

            return View(viewModel);
        }

        [HttpPost]
        [Permission("Roles.Edit")]
        public async Task<IActionResult> Edit(EditRoleViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var rolePermissionIds = await _context.RolePermissions
                    .Where(rp => rp.RoleId == viewModel.Id)
                    .Select(rp => rp.PermissionId)
                    .ToListAsync();
                viewModel.PermissionGroups = await GetPermissionGroupsAsync(rolePermissionIds);
                return View(viewModel);
            }

            var role = await _roleManager.FindByIdAsync(viewModel.Id);
            if (role == null)
            {
                TempData["error-notification"] = _localizer["roleNotFound"].Value;
                return RedirectToAction(nameof(Index));
            }

            if (role.IsSystemRole && role.Name != viewModel.Name)
            {
                ModelState.AddModelError("Name", "Cannot change system role name");
                var rolePermissionIds = await _context.RolePermissions
                    .Where(rp => rp.RoleId == viewModel.Id)
                    .Select(rp => rp.PermissionId)
                    .ToListAsync();
                viewModel.PermissionGroups = await GetPermissionGroupsAsync(rolePermissionIds);
                return View(viewModel);
            }

            role.Name = viewModel.Name;
            role.Description = viewModel.Description;
            role.LastModified = DateTime.UtcNow;

            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                // Update permissions
                var existingPermissions = await _context.RolePermissions
                    .Where(rp => rp.RoleId == viewModel.Id)
                    .ToListAsync();

                _context.RolePermissions.RemoveRange(existingPermissions);

                if (viewModel.SelectedPermissionIds != null && viewModel.SelectedPermissionIds.Any())
                {
                    var newRolePermissions = viewModel.SelectedPermissionIds.Select(permissionId => new RolePermission
                    {
                        RoleId = viewModel.Id,
                        PermissionId = permissionId
                    }).ToList();

                    await _context.RolePermissions.AddRangeAsync(newRolePermissions);
                }

                await _context.SaveChangesAsync();

                TempData["success-notification"] = _localizer["updateRoleSuccess"].Value;
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            var currentRolePermissionIds = await _context.RolePermissions
                .Where(rp => rp.RoleId == viewModel.Id)
                .Select(rp => rp.PermissionId)
                .ToListAsync();
            viewModel.PermissionGroups = await GetPermissionGroupsAsync(currentRolePermissionIds);
            return View(viewModel);
        }

        [HttpPost]
        [Permission("Roles.Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                TempData["error-notification"] = _localizer["roleNotFound"].Value;
                return RedirectToAction(nameof(Index));
            }

            if (role.IsSystemRole)
            {
                TempData["error-notification"] = _localizer["cannotDeleteSystemRole"].Value;
                return RedirectToAction(nameof(Index));
            }

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            if (usersInRole.Any())
            {
                TempData["error-notification"] = _localizer["cannotDeleteRoleWithUsers"].Value;
                return RedirectToAction(nameof(Index));
            }

            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
            {
                TempData["success-notification"] = _localizer["deleteRoleSuccess"].Value;
            }
            else
            {
                TempData["error-notification"] = _localizer["deleteRoleFailed"].Value;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Permission("Roles.Manage")]
        public async Task<IActionResult> ManagePermissions(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                TempData["error-notification"] = _localizer["roleNotFound"].Value;
                return RedirectToAction(nameof(Index));
            }

            var allPermissions = await _context.Permissions
                .OrderBy(p => p.Module)
                .ThenBy(p => p.Action)
                .ToListAsync();

            var rolePermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == id)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            var permissionGroups = allPermissions
                .GroupBy(p => p.Module)
                .Select(g => new PermissionGroupViewModel
                {
                    Module = g.Key,
                    Permissions = g.Select(p => new PermissionItemViewModel
                    {
                        Id = p.Id,
                        Name = p.Name,
                        DisplayName = p.DisplayName,
                        Description = p.Description,
                        Action = p.Action,
                        IsAssigned = rolePermissions.Contains(p.Id)
                    }).ToList()
                }).ToList();

            var viewModel = new RolePermissionsViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name!,
                IsSystemRole = role.IsSystemRole,
                PermissionGroups = permissionGroups
            };

            return View(viewModel);
        }

        [HttpPost]
        [Permission("Roles.Manage")]
        public async Task<IActionResult> UpdatePermissions(string roleId, List<int> permissionIds)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return Json(new { success = false, message = "Role not found" });
            }

            // Remove all existing permissions for this role
            var existingPermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();

            _context.RolePermissions.RemoveRange(existingPermissions);

            // Add new permissions
            if (permissionIds != null && permissionIds.Any())
            {
                var newRolePermissions = permissionIds.Select(permissionId => new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId
                }).ToList();

                await _context.RolePermissions.AddRangeAsync(newRolePermissions);
            }

            await _context.SaveChangesAsync();

            TempData["success-notification"] = _localizer["updatePermissionsSuccess"].Value;
            return Json(new { success = true });
        }

        private async Task<List<PermissionGroupViewModel>> GetPermissionGroupsAsync(List<int>? selectedPermissionIds = null)
        {
            var allPermissions = await _context.Permissions
                .OrderBy(p => p.Module)
                .ThenBy(p => p.Action)
                .ToListAsync();

            var permissionGroups = allPermissions
                .GroupBy(p => p.Module)
                .Select(g => new PermissionGroupViewModel
                {
                    Module = g.Key,
                    Permissions = g.Select(p => new PermissionItemViewModel
                    {
                        Id = p.Id,
                        Name = p.Name,
                        DisplayName = p.DisplayName,
                        Description = p.Description,
                        Action = p.Action,
                        IsAssigned = selectedPermissionIds != null && selectedPermissionIds.Contains(p.Id)
                    }).ToList()
                }).ToList();

            return permissionGroups;
        }
    }
}

