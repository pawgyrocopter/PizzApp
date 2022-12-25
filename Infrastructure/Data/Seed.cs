using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class Seed
{
    public static async Task SeedUsers(UserManager<User> userManager, RoleManager<Role> roleManager)
    {
        if (await userManager.Users.AnyAsync()) return;

        var roles = new List<Role>()
        {
            new() {Id = 1, Name = "Customer"},
            new() {Id = 2, Name = "PizzaMaker"},
            new() {Id = 3, Name = "Admin"}
        };
        
        foreach (var role in roles) await roleManager.CreateAsync(role);
        
        // for (int i = 0; i < 10; i++)
        // {
        //     var user = new User() {Id = i + 1, UserName = $"user{i + 1}", Email = $"user{i + 1}@gmail.com"};
        //     await userManager.CreateAsync(user, "123123");
        //     await userManager.AddToRoleAsync(user, "Customer");
        // }

        var admin = new User
        {
            Id = 11,
            UserName = "admin",
            NormalizedUserName = "ADMIN",
            Email = "admin",
            NormalizedEmail = "admin",
            EmailConfirmed = false,
            PhoneNumber = "admin",
            PhoneNumberConfirmed = true,
            TwoFactorEnabled = false,
            LockoutEnd = null,
            LockoutEnabled = false,
            AccessFailedCount = 0,
            Adress = "admin"
        };
        
        await userManager.CreateAsync(admin, "123123");
        await userManager.AddToRolesAsync(admin, new[] {"Admin", "PizzaMaker", "Customer"});
    }
}