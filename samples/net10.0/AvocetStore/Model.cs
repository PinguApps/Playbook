// snippet:complete
using Microsoft.EntityFrameworkCore;
namespace Avocet {
public sealed class AvocetContext : DbContext {
    public DbSet<Sighting> Sightings => Set<Sighting>();
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite(Environment.GetEnvironmentVariable("PLAYBOOK_AVOCET_DATABASE") ?? "Data Source=avocet.db");
}
public sealed class Sighting { public int Id { get; set; } public string Ring { get; set; } = ""; }
}
// endsnippet:complete
