using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Dyadic.Infrastructure.Data;

public static class RoleSeeder {
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider) {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        string[] roles = ["Student", "Supervisor", "Admin"];

        foreach (var role in roles) {
            if (!await roleManager.RoleExistsAsync(role)) {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }
    }
}