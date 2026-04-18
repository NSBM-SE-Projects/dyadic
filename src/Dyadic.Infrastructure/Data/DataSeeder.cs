namespace Dyadic.Infrastructure.Data;

public static class DataSeeder {
    public static async Task SeedAsync(IServiceProvider serviceProvider) {
        await RoleSeeder.SeedRolesAsync(serviceProvider);
        await AdminSeeder.SeedAdminAsync(serviceProvider);
    }
}
