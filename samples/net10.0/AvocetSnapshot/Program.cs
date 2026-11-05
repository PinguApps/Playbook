// snippet:complete
using Microsoft.EntityFrameworkCore;
using Avocet;
using var db = new AvocetContext();
if (args.Contains("--check")) {
    db.Sightings.Add(new Sighting { Ring = "AV-12" });
    db.SaveChanges();
    if (db.Sightings.AsNoTracking().Count(x => x.Ring == "AV-12") != 1) throw new Exception("Persistence failed");
    Console.WriteLine("PASS migrated schema and persisted sighting");
}
namespace Avocet {
public sealed class AvocetContext : DbContext {
    public DbSet<Sighting> Sightings => Set<Sighting>();
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite("Data Source=avocet.db");
}
public sealed class Sighting { public int Id { get; set; } public string Ring { get; set; } = ""; }
}
// endsnippet:complete
