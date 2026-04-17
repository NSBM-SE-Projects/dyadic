using Dyadic.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Dyadic.Infrastructure.Data;

public static class AdminSeeder { 
    public static async Task SeedAdminAsync(IServiceProvider serviceProvider) {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        const string adminEmail = "admin@dyadic.lk";

        if (await userManager.FindByEmailAsync(adminEmail) != null)
            return;

        var admin = new ApplicationUser {
            FullName = "System Administrator",
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var config = serviceProvider.GetRequiredService<IConfiguration>();
        var password = config["SeedAdmin:Password"] ?? "Admin123456";
        var result = await userManager.CreateAsync(admin, password);

        if (result.Succeeded) {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}