using Dyadic.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Dyadic.IntegrationTests.Fixtures;

/// <summary>
/// Creates a per-test isolated SQL Server database, applies all migrations,
/// and drops the database on disposal. Requires the environment variable
/// ConnectionStrings__DefaultConnection to point at a running SQL Server instance.
/// </summary>
public class SqlServerDbContextFactory : IAsyncDisposable
{
    private readonly string _connectionString;
    private ApplicationDbContext? _context;

    public SqlServerDbContextFactory()
    {
        var baseConnection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? throw new InvalidOperationException(
                "ConnectionStrings__DefaultConnection is not set. " +
                "Start Docker ('make up') and set the env var before running SQL Server tests.");

        var dbName = $"Dyadic_Test_{Guid.NewGuid():N}";
        _connectionString = ReplaceDatabase(baseConnection, dbName);
    }

    public async Task<ApplicationDbContext> CreateAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        _context = new ApplicationDbContext(options);
        await _context.Database.MigrateAsync();
        return _context;
    }

    public async ValueTask DisposeAsync()
    {
        if (_context != null)
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.DisposeAsync();
        }
    }

    private static string ReplaceDatabase(string connectionString, string newDbName)
    {
        // Replace Database=... or Initial Catalog=... with the test DB name
        var result = System.Text.RegularExpressions.Regex.Replace(
            connectionString,
            @"(Database|Initial Catalog)=[^;]+",
            $"$1={newDbName}",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        // If no Database= found, append it
        if (!result.Contains("Database=", StringComparison.OrdinalIgnoreCase) &&
            !result.Contains("Initial Catalog=", StringComparison.OrdinalIgnoreCase))
            result += $";Database={newDbName}";

        return result;
    }
}
