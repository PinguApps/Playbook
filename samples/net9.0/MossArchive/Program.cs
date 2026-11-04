// snippet:complete
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MossArchive;

internal static class Program
{
    private static void Main()
    {
        using SqliteConnection connection = new("Data Source=:memory:");
        connection.Open();
        DbContextOptions<ArchiveContext> options = new DbContextOptionsBuilder<ArchiveContext>().UseSqlite(connection).Options;
        using (ArchiveContext setup = new(options, "moss", false))
        {
            setup.Database.EnsureCreated();
            setup.AddRange(new LedgerEntry { Id = 1, Tenant = "moss" }, new LedgerEntry { Id = 2, Tenant = "moss", Archived = true }, new LedgerEntry { Id = 3, Tenant = "fern" }, new LedgerEntry { Id = 4, Tenant = "fern", Archived = true });
            setup.SaveChanges();
        }
        foreach (string tenant in new[] { "moss", "fern" })
        {
            using ArchiveContext normal = new(options, tenant, false);
            using ArchiveContext history = new(options, tenant, true);
            int activeId = tenant == "moss" ? 1 : 3;
            Check(normal.Entries.AsNoTracking().Select(x => x.Id).ToArray(), [activeId]);
            Check(history.History().OrderBy(x => x.Id).Select(x => x.Id).ToArray(), [activeId, activeId + 1]);
            if (normal.Entries.IgnoreQueryFilters().Count() != 4) { throw new InvalidOperationException("Negative control failed."); }
        }
        Console.WriteLine("PASS: real SQLite active/history reads for both tenants; archival toggle retains tenant predicate; disabling all filters exposes four rows. Query filters are not authorization or write protection.");
    }

    private static void Check(int[] actual, int[] expected)
    {
        if (!actual.SequenceEqual(expected)) { throw new InvalidOperationException("Unexpected tenant/history rows."); }
    }
}

internal sealed class LedgerEntry
{
    public int Id { get; set; }
    public string Tenant { get; set; } = "";
    public bool Archived { get; set; }
}

internal sealed class ArchiveContext(DbContextOptions<ArchiveContext> options, string tenant, bool includeArchived) : DbContext(options)
{
    public DbSet<LedgerEntry> Entries => Set<LedgerEntry>();
    public IQueryable<LedgerEntry> History() => Entries.AsNoTracking();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LedgerEntry>().HasQueryFilter(x => x.Tenant == tenant && (includeArchived || !x.Archived));
    }
}

// endsnippet:complete
