using Microsoft.EntityFrameworkCore;

namespace Dyadic.Infrastructure;

public class ApplicationDbContext: DbContext {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        :base(options)
    {
    }

    // DbSet<T> properties go here as entities are added
    // Example: public DbSet<Proposal> Proposals => Set<Proposal>();
}