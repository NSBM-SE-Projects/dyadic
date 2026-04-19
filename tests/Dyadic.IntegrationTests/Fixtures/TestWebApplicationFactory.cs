using Dyadic.Application.Services;
using Dyadic.Domain.Entities;
using Dyadic.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace Dyadic.IntegrationTests.Fixtures;

/// <summary>
/// WebApplicationFactory that swaps SQL Server for InMemory and seeds Identity roles + admin on startup.
/// Each factory instance gets its own isolated InMemory database.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"E2E_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });
    }

    /// <summary>Creates a user via UserManager and assigns the given role.</summary>
    public async Task<ApplicationUser> SeedUserAsync(
        string fullName, string email, string role, string password = "Test@123456")
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            FullName = fullName,
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                $"Seed user failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        await userManager.AddToRoleAsync(user, role);
        return user;
    }

    /// <summary>Creates an active research area via IResearchAreaService and returns its ID.</summary>
    public async Task<Guid> SeedResearchAreaAsync(string name = "Computer Science")
    {
        using var scope = Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IResearchAreaService>();
        var area = await service.CreateAsync(name);
        return area.Id;
    }

    /// <summary>
    /// Creates an HttpClient authenticated as the given user.
    /// Fetches the login page to obtain the antiforgery token, then POSTs credentials.
    /// Returns a client with the auth cookie already set.
    /// Uses HTTPS base address to avoid the HTTP→HTTPS redirect from UseHttpsRedirection.
    /// </summary>
    public async Task<HttpClient> GetAuthenticatedClientAsync(
        string email, string password = "Test@123456")
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        var getResponse = await client.GetAsync("/Account/Login");
        var html = await getResponse.Content.ReadAsStringAsync();
        var token = ExtractAntiforgeryToken(html);

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Email"]                = email,
            ["Input.Password"]             = password,
            ["Input.RememberMe"]           = "false",
            ["__RequestVerificationToken"] = token
        });

        await client.PostAsync("/Account/Login", form);
        return client;
    }

    /// <summary>Extracts the antiforgery token from a Razor Pages HTML response.</summary>
    public static string ExtractAntiforgeryToken(string html)
    {
        var match = Regex.Match(html,
            @"<input[^>]+name=""__RequestVerificationToken""[^>]+value=""([^""]+)""");
        if (!match.Success)
            match = Regex.Match(html,
                @"<input[^>]+value=""([^""]+)""[^>]+name=""__RequestVerificationToken""");
        return match.Success ? match.Groups[1].Value : string.Empty;
    }
}
