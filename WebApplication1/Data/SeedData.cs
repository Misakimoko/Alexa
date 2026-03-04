using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Lexa.Entities;

namespace Lexa.Data;

public static class SeedData
{
    /// <summary>
    /// Seed initial users, roles and permissions.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Ensure database is created / migrations applied before seeding (should be handled externally)
        await db.Database.EnsureCreatedAsync();

        // Create permissions
        var permNames = new List<string>()
        {
            "role:manage",
            "permission:manage",
            "role:assign",
            "role:permission:assign",
            "user:manage",
            "user:view"
        };

        foreach (var name in permNames)
        {
            if (!await db.Permissions.AnyAsync(p => p.Name == name))
            {
                db.Permissions.Add(new Permission { Name = name });
            }
        }

        // Create Admin role
        var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole == null)
        {
            adminRole = new Role { Name = "Admin" };
            db.Roles.Add(adminRole);
        }

        await db.SaveChangesAsync();

        // Attach all perms to Admin role
        var perms = await db.Permissions.Where(p => permNames.Contains(p.Name)).ToListAsync();
        foreach (var p in perms)
        {
            var exists = await db.RolePermissions.FindAsync(adminRole.Id, p.Id);
            if (exists == null)
            {
                db.RolePermissions.Add(new RolePermission { RoleId = adminRole.Id, PermissionId = p.Id });
            }
        }

        // Create admin user
        var adminUser = await db.Users.FirstOrDefaultAsync(u => u.UserName == "admin");
        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = "admin",
                Email = "admin@local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123")
            };
            db.Users.Add(adminUser);
            await db.SaveChangesAsync();
        }

        // Assign admin role to admin user
        var urExists = await db.UserRoles.FindAsync(adminUser.Id, adminRole.Id);
        if (urExists == null)
        {
            db.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id });
        }

        await db.SaveChangesAsync();
    }
}
