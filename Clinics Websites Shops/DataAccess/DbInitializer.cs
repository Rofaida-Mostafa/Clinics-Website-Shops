using Clinics_Websites_Shops.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Clinics_Websites_Shops.DataAccess
{
    public class DbInitializer
    {
        public static async Task SeedRolesAndPermissions(
            ApplicationDbContext context,
            RoleManager<ApplicationRole> roleManager)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Seed Permissions first
            if (!await context.Permissions.AnyAsync())
            {
                var permissions = GetDefaultPermissions();
                await context.Permissions.AddRangeAsync(permissions);
                await context.SaveChangesAsync();
            }

            // Seed Roles
            if (!await roleManager.Roles.AnyAsync())
            {
                var roles = GetDefaultRoles();
                foreach (var role in roles)
                {
                    await roleManager.CreateAsync(role);
                }
            }

            // Assign Permissions to Roles
            await AssignPermissionsToRoles(context);
        }

        private static List<Permission> GetDefaultPermissions()
        {
            var permissions = new List<Permission>();
            var modules = new[] { "Doctors", "Nurses", "Patients", "Appointments", "Departments", "Reports", "Payments", "Users", "Roles" };
            var actions = new[] { "View", "Create", "Edit", "Delete", "Manage" };

            foreach (var module in modules)
            {
                foreach (var action in actions)
                {
                    // Skip "Manage" for most modules (only for Users and Roles)
                    if (action == "Manage" && module != "Users" && module != "Roles")
                        continue;

                    permissions.Add(new Permission
                    {
                        Name = $"{module}.{action}",
                        DisplayName = $"{action} {module}",
                        Description = $"Permission to {action.ToLower()} {module.ToLower()}",
                        Module = module,
                        Action = action,
                        IsActive = true
                    });
                }
            }

            return permissions;
        }

        private static List<ApplicationRole> GetDefaultRoles()
        {
            return new List<ApplicationRole>
            {
                new ApplicationRole
                {
                    Name = "SuperAdmin",
                    NormalizedName = "SUPERADMIN",
                    Description = "Super Administrator with full system access",
                    IsSystemRole = true
                },
                new ApplicationRole
                {
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    Description = "Administrator with management access",
                    IsSystemRole = true
                },
                new ApplicationRole
                {
                    Name = "Doctor",
                    NormalizedName = "DOCTOR",
                    Description = "Doctor role with medical access",
                    IsSystemRole = true
                },
                new ApplicationRole
                {
                    Name = "Nurse",
                    NormalizedName = "NURSE",
                    Description = "Nurse role with patient care access",
                    IsSystemRole = true
                },
                new ApplicationRole
                {
                    Name = "Accountant",
                    NormalizedName = "ACCOUNTANT",
                    Description = "Accountant role with financial access",
                    IsSystemRole = true
                },
                new ApplicationRole
                {
                    Name = "Receptionist",
                    NormalizedName = "RECEPTIONIST",
                    Description = "Receptionist role with appointment management",
                    IsSystemRole = true
                }
            };
        }

        private static async Task AssignPermissionsToRoles(ApplicationDbContext context)
        {
            // Check if permissions are already assigned
            if (await context.RolePermissions.AnyAsync())
                return;

            var roles = await context.Roles.ToListAsync();
            var permissions = await context.Permissions.ToListAsync();

            var rolePermissions = new List<RolePermission>();

            // SuperAdmin - All permissions
            var superAdminRole = roles.FirstOrDefault(r => r.Name == "SuperAdmin");
            if (superAdminRole != null)
            {
                foreach (var permission in permissions)
                {
                    rolePermissions.Add(new RolePermission
                    {
                        RoleId = superAdminRole.Id,
                        PermissionId = permission.Id
                    });
                }
            }

            // Admin - All except User/Role management
            var adminRole = roles.FirstOrDefault(r => r.Name == "Admin");
            if (adminRole != null)
            {
                var adminPermissions = permissions.Where(p =>
                    p.Module != "Users" && p.Module != "Roles").ToList();

                foreach (var permission in adminPermissions)
                {
                    rolePermissions.Add(new RolePermission
                    {
                        RoleId = adminRole.Id,
                        PermissionId = permission.Id
                    });
                }
            }

            // Doctor - View/Edit Patients, Appointments, Reports
            var doctorRole = roles.FirstOrDefault(r => r.Name == "Doctor");
            if (doctorRole != null)
            {
                var doctorPermissions = permissions.Where(p =>
                    (p.Module == "Patients" && (p.Action == "View" || p.Action == "Edit")) ||
                    (p.Module == "Appointments" && (p.Action == "View" || p.Action == "Edit")) ||
                    (p.Module == "Reports" && (p.Action == "View" || p.Action == "Create" || p.Action == "Edit")) ||
                    (p.Module == "Doctors" && p.Action == "View")
                ).ToList();

                foreach (var permission in doctorPermissions)
                {
                    rolePermissions.Add(new RolePermission
                    {
                        RoleId = doctorRole.Id,
                        PermissionId = permission.Id
                    });
                }
            }

            // Nurse - View/Edit Patients, View Appointments
            var nurseRole = roles.FirstOrDefault(r => r.Name == "Nurse");
            if (nurseRole != null)
            {
                var nursePermissions = permissions.Where(p =>
                    (p.Module == "Patients" && (p.Action == "View" || p.Action == "Edit")) ||
                    (p.Module == "Appointments" && p.Action == "View") ||
                    (p.Module == "Nurses" && p.Action == "View")
                ).ToList();

                foreach (var permission in nursePermissions)
                {
                    rolePermissions.Add(new RolePermission
                    {
                        RoleId = nurseRole.Id,
                        PermissionId = permission.Id
                    });
                }
            }

            // Accountant - View/Edit Payments, View Appointments
            var accountantRole = roles.FirstOrDefault(r => r.Name == "Accountant");
            if (accountantRole != null)
            {
                var accountantPermissions = permissions.Where(p =>
                    (p.Module == "Payments") ||
                    (p.Module == "Appointments" && p.Action == "View")
                ).ToList();

                foreach (var permission in accountantPermissions)
                {
                    rolePermissions.Add(new RolePermission
                    {
                        RoleId = accountantRole.Id,
                        PermissionId = permission.Id
                    });
                }
            }

            // Receptionist - Manage Appointments, View Patients/Doctors
            var receptionistRole = roles.FirstOrDefault(r => r.Name == "Receptionist");
            if (receptionistRole != null)
            {
                var receptionistPermissions = permissions.Where(p =>
                    (p.Module == "Appointments") ||
                    (p.Module == "Patients" && p.Action == "View") ||
                    (p.Module == "Doctors" && p.Action == "View")
                ).ToList();

                foreach (var permission in receptionistPermissions)
                {
                    rolePermissions.Add(new RolePermission
                    {
                        RoleId = receptionistRole.Id,
                        PermissionId = permission.Id
                    });
                }
            }

            await context.RolePermissions.AddRangeAsync(rolePermissions);
            await context.SaveChangesAsync();
        }
    }
}

