// snippet:complete
using Microsoft.EntityFrameworkCore;
using var db = new CurlewDesign.SurveyContext();
if (args.Contains("--check")) {
    db.Nests.Add(new CurlewDesign.Nest { Code = "CUR-7" }); db.SaveChanges();
    if (db.Nests.AsNoTracking().Count(n => n.Code == "CUR-7") != 1) throw new Exception("Persistence failed");
    Console.WriteLine("PASS migrated schema and persisted project row");
}
namespace CurlewDesign {
public sealed class SurveyContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite("Data Source=curlew.db");
    public DbSet<Nest> Nests => Set<Nest>();
}
public sealed class Nest { public int Id { get; set; } public string Code { get; set; } = ""; }
}
// endsnippet:complete
