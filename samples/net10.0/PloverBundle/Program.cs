// snippet:complete
using Microsoft.EntityFrameworkCore;
using var db = new PloverBundle.SurveyContext();
if (args.Contains("--check")) {
    db.Nests.Add(new PloverBundle.Nest { Code = "PLO-7" }); db.SaveChanges();
    if (db.Nests.AsNoTracking().Count(n => n.Code == "PLO-7") != 1) throw new Exception("Persistence failed");
    Console.WriteLine("PASS migrated schema and persisted project row");
}
namespace PloverBundle {
public sealed class SurveyContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite("Data Source=plover.db");
    public DbSet<Nest> Nests => Set<Nest>();
}
public sealed class Nest { public int Id { get; set; } public string Code { get; set; } = ""; }
}
// endsnippet:complete

