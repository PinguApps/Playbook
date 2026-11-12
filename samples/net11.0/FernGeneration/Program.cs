// snippet:complete
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FernGeneration;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        await using LedgerContext context = new();
        if (args.Contains("--require-model", StringComparer.Ordinal) && context.Model.GetType().Name != "LedgerContextModel") throw new InvalidOperationException("Compiled model was not loaded.");
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();
        context.Add(new SeedPacket { Name = "Fern spores" });
        await context.SaveChangesAsync();
        List<SeedPacket> packets = await context.Packets.AsNoTracking().Where(x => x.Id > 0).ToListAsync();
        if (packets.Count != 1 || packets[0].Name != "Fern spores") { throw new InvalidOperationException("Query result mismatch."); }
        Console.WriteLine("PASS: SQLite generated-consumer query returns the stored seed packet; no NativeAOT execution claim.");
    }
}
public class SeedPacket
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}
public sealed class LedgerContext : DbContext
{
    public DbSet<SeedPacket> Packets => Set<SeedPacket>();
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite("Data Source=:memory:");
}
public sealed class LedgerFactory : IDesignTimeDbContextFactory<LedgerContext>
{
    public LedgerContext CreateDbContext(string[] args) => new();
}
// endsnippet:complete
