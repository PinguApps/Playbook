// snippet:complete
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
Console.WriteLine("Beacon migrations authoring consumer; use the isolated scaffolding check.");
namespace BeaconSamples {
public sealed class BeaconContext : DbContext {
 protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite("Data Source=beacon.db");
 protected override void OnModelCreating(ModelBuilder modelBuilder) {
  var e = modelBuilder.Entity<Beacon>(); e.HasKey(x => x.Id);
  // branch-properties
 }
}
public sealed class Beacon { public int Id { get; set; } }
public sealed class Factory : IDesignTimeDbContextFactory<BeaconContext> {
 public BeaconContext CreateDbContext(string[] args) => new();
}
}
// endsnippet:complete

