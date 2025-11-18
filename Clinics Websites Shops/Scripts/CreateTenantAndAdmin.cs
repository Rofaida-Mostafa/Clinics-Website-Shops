/*
 * Script to create a new tenant and admin user
 * 
 * To run this script:
 * 1. Add this code to Program.cs temporarily, or
 * 2. Create a new endpoint in a controller to execute this
 * 
 * Tenant Details:
 * - Name: Test Clinic
 * - Domain: test.localhost
 * - Admin Email: test@test.com
 * - Admin Password: 123456
 */

using Clinics_Websites_Shops.DataAccess;
using Clinics_Websites_Shops.Models;
using Clinics_Websites_Shops.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public static class TenantSeeder
{
    public static async Task CreateTestTenantAndAdmin(
        MasterDbContext masterDb,
        ApplicationDbContext appDb,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        try
        {
            Console.WriteLine("=== Creating Test Tenant and Admin User ===");
            
            // Step 1: Check if tenant already exists
            var existingTenant = await masterDb.Tenants
                .FirstOrDefaultAsync(t => t.Domain == "test.localhost");
            
            string tenantId;
            
            if (existingTenant != null)
            {
                Console.WriteLine($"Tenant already exists with ID: {existingTenant.TId}");
                tenantId = existingTenant.TId;
            }
            else
            {
                // Create new tenant
                tenantId = Guid.NewGuid().ToString();
                
                var tenant = new Tenant
                {
                    TId = tenantId,
                    Name = "Test Clinic",
                    Domain = "test.localhost",
                    ConnectionString = "Server=localhost;Port=3306;Database=ClinicsWebsiteShops;User=root;Password=;",
                    Status = true,
                    Locals = "en,ar",
                    CreatedAt = DateTime.UtcNow
                };
                
                await masterDb.Tenants.AddAsync(tenant);
                await masterDb.SaveChangesAsync();
                
                Console.WriteLine($"✓ Tenant created successfully with ID: {tenantId}");
            }
            
            // Step 2: Ensure SuperAdmin role exists
            var superAdminRole = await roleManager.FindByNameAsync("SuperAdmin");
            if (superAdminRole == null)
            {
                superAdminRole = new ApplicationRole
                {
                    Name = "SuperAdmin",
                    NormalizedName = "SUPERADMIN",
                    Description = "Super Administrator with full system access",
                    IsSystemRole = true
                };
                
                var roleResult = await roleManager.CreateAsync(superAdminRole);
                if (roleResult.Succeeded)
                {
                    Console.WriteLine("✓ SuperAdmin role created");
                }
                else
                {
                    Console.WriteLine("✗ Failed to create SuperAdmin role");
                    foreach (var error in roleResult.Errors)
                    {
                        Console.WriteLine($"  - {error.Description}");
                    }
                    return;
                }
            }
            
            // Step 3: Check if admin user already exists
            var existingUser = await userManager.FindByEmailAsync("test@test.com");
            
            if (existingUser != null)
            {
                Console.WriteLine($"User already exists: {existingUser.Email}");
                
                // Update tenant ID if needed
                if (existingUser.TenantId != tenantId)
                {
                    existingUser.TenantId = tenantId;
                    await userManager.UpdateAsync(existingUser);
                    Console.WriteLine("✓ User tenant ID updated");
                }
                
                // Ensure user has SuperAdmin role
                if (!await userManager.IsInRoleAsync(existingUser, "SuperAdmin"))
                {
                    await userManager.AddToRoleAsync(existingUser, "SuperAdmin");
                    Console.WriteLine("✓ SuperAdmin role assigned to user");
                }
            }
            else
            {
                // Create new admin user
                var adminUser = new ApplicationUser
                {
                    UserName = "test@test.com",
                    Email = "test@test.com",
                    Name = "Test Admin",
                    Description = "Test Administrator",
                    Rate = 0,
                    TenantId = tenantId,
                    EmailConfirmed = true
                };
                
                var userResult = await userManager.CreateAsync(adminUser, "123456");
                
                if (userResult.Succeeded)
                {
                    Console.WriteLine("✓ Admin user created successfully");
                    
                    // Assign SuperAdmin role
                    var roleAssignResult = await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                    if (roleAssignResult.Succeeded)
                    {
                        Console.WriteLine("✓ SuperAdmin role assigned to user");
                    }
                    else
                    {
                        Console.WriteLine("✗ Failed to assign SuperAdmin role");
                        foreach (var error in roleAssignResult.Errors)
                        {
                            Console.WriteLine($"  - {error.Description}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("✗ Failed to create admin user");
                    foreach (var error in userResult.Errors)
                    {
                        Console.WriteLine($"  - {error.Description}");
                    }
                    return;
                }
            }
            
            Console.WriteLine("\n=== Summary ===");
            Console.WriteLine($"Tenant ID: {tenantId}");
            Console.WriteLine($"Tenant Name: Test Clinic");
            Console.WriteLine($"Domain: test.localhost");
            Console.WriteLine($"Admin Email: test@test.com");
            Console.WriteLine($"Admin Password: 123456");
            Console.WriteLine("\nYou can now login at: http://test.localhost:5292/Identity/Account/Login");
            Console.WriteLine("(Make sure to add 'test.localhost' to your hosts file)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
    }
}

