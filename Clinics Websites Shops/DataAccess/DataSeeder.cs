using Clinics_Websites_Shops.Models;
using Clinics_Websites_Shops.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Clinics_Websites_Shops.DataAccess
{
    public class DataSeeder
    {
        public static async Task SeedAllData(
            MasterDbContext masterDb,
            ApplicationDbContext appDb,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            Console.WriteLine("=== Starting Data Seeding ===");

            // 1. Seed Permissions
            await SeedPermissions(appDb);

            // 2. Seed Roles
            await SeedRoles(roleManager);

            // 3. Assign Permissions to Roles
            await AssignPermissionsToRoles(appDb);

            Console.WriteLine("=== Data Seeding Completed ===");
        }

        private static async Task SeedPermissions(ApplicationDbContext context)
        {
            if (await context.Permissions.AnyAsync())
            {
                Console.WriteLine("✓ Permissions already exist");
                return;
            }

            Console.WriteLine("Creating permissions...");

            var permissions = new List<Permission>();
            var modules = new[] { 
                "Doctors", "Nurses", "Patients", "Appointments", "Departments", 
                "Reports", "Payments", "Users", "Roles", "Prescriptions", 
                "Results", "Settings" 
            };
            var actions = new[] { "View", "Create", "Edit", "Delete", "Manage" };

            foreach (var module in modules)
            {
                foreach (var action in actions)
                {
                    // Skip "Manage" for most modules
                    if (action == "Manage" && module != "Users" && module != "Roles" && module != "Settings")
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

            await context.Permissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();

            Console.WriteLine($"✓ Created {permissions.Count} permissions");
        }

        private static async Task SeedRoles(RoleManager<ApplicationRole> roleManager)
        {
            if (await roleManager.Roles.AnyAsync())
            {
                Console.WriteLine("✓ Roles already exist");
                return;
            }

            Console.WriteLine("Creating roles...");

            var roles = new[]
            {
                new ApplicationRole
                {
                    Name = "SuperAdmin",
                    Description = "Super Administrator with full system access",
                    IsSystemRole = true
                },
                new ApplicationRole
                {
                    Name = "Doctor",
                    Description = "Medical Doctor with patient care permissions",
                    IsSystemRole = true
                },
                new ApplicationRole
                {
                    Name = "Nurse",
                    Description = "Nurse with patient care support permissions",
                    IsSystemRole = true
                },
                new ApplicationRole
                {
                    Name = "Receptionist",
                    Description = "Front desk staff with appointment management",
                    IsSystemRole = false
                },
                new ApplicationRole
                {
                    Name = "Accountant",
                    Description = "Financial staff with payment management",
                    IsSystemRole = false
                }
            };

            foreach (var role in roles)
            {
                var result = await roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    Console.WriteLine($"✓ Created role: {role.Name}");
                }
            }
        }

        private static async Task AssignPermissionsToRoles(ApplicationDbContext context)
        {
            if (await context.RolePermissions.AnyAsync())
            {
                Console.WriteLine("✓ Role permissions already assigned");
                return;
            }

            Console.WriteLine("Assigning permissions to roles...");

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
                Console.WriteLine($"✓ Assigned all permissions to SuperAdmin");
            }

            // Doctor - Full access to Patients, Appointments, Reports, Prescriptions, Results
            var doctorRole = roles.FirstOrDefault(r => r.Name == "Doctor");
            if (doctorRole != null)
            {
                var doctorPermissions = permissions.Where(p =>
                    (p.Module == "Patients" && (p.Action == "View" || p.Action == "Edit")) ||
                    (p.Module == "Appointments" && (p.Action == "View" || p.Action == "Edit" || p.Action == "Create")) ||
                    (p.Module == "Reports" && (p.Action == "View" || p.Action == "Create" || p.Action == "Edit")) ||
                    (p.Module == "Prescriptions" && (p.Action == "View" || p.Action == "Create" || p.Action == "Edit" || p.Action == "Delete")) ||
                    (p.Module == "Results" && (p.Action == "View" || p.Action == "Create" || p.Action == "Edit" || p.Action == "Delete")) ||
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
                Console.WriteLine($"✓ Assigned {doctorPermissions.Count} permissions to Doctor");
            }

            // Nurse - View/Edit Patients, View Appointments, View Prescriptions/Results
            var nurseRole = roles.FirstOrDefault(r => r.Name == "Nurse");
            if (nurseRole != null)
            {
                var nursePermissions = permissions.Where(p =>
                    (p.Module == "Patients" && (p.Action == "View" || p.Action == "Edit")) ||
                    (p.Module == "Appointments" && p.Action == "View") ||
                    (p.Module == "Prescriptions" && p.Action == "View") ||
                    (p.Module == "Results" && p.Action == "View") ||
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
                Console.WriteLine($"✓ Assigned {nursePermissions.Count} permissions to Nurse");
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
                Console.WriteLine($"✓ Assigned {receptionistPermissions.Count} permissions to Receptionist");
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
                Console.WriteLine($"✓ Assigned {accountantPermissions.Count} permissions to Accountant");
            }

            await context.RolePermissions.AddRangeAsync(rolePermissions);
            await context.SaveChangesAsync();

            Console.WriteLine($"✓ Total role permissions created: {rolePermissions.Count}");
        }
    }
}

