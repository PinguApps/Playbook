// snippet:complete
using System.Linq.Expressions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;

using var connection = new SqliteConnection("Data Source=:memory:");
connection.Open();
var options = new DbContextOptionsBuilder<NestContext>().UseSqlite(connection).Options;
using (var db = new NestContext(options))
{
    db.Database.EnsureCreated();
    db.AddRange(new Nest { Id = 1, Location = new Location { Ridge = "West" }, Note = null },
        new Nest { Id = 2, Location = new Location { Ridge = "East" }, Note = "Occupied" });
    db.SaveChanges();
}
using (var db = new NestContext(options))
{
    // A bounded extension consumer obtains the current public structural contract.
    var source = db.GetService<IStructuralTypeMaterializerSource>();
    var entity = db.Model.FindEntityType(typeof(Nest))!;
    if (source.GetMaterializer(entity) is null) throw new Exception("Missing entity materializer.");
    var rows = db.Set<Nest>().AsNoTracking().OrderBy(x => x.Id).ToArray();
    if (rows.Length != 2 || rows[0].Location.Ridge != "West" || rows[0].Note is not null
        || rows[1].Note != "Occupied") throw new Exception("Entity/nested/null materialization changed.");
#if NET10_0
#pragma warning disable CS0618
    var root = new ParameterQueryRootExpression(typeof(int), Expression.Parameter(typeof(int[]), "counts"));
    if (root.ParameterExpression.Name != "counts") throw new Exception("Old root lost parameter.");
#pragma warning restore CS0618
#else
    var root = new ParameterQueryRootExpression(typeof(int), new QueryParameterExpression("counts", typeof(int[])));
    if (root.QueryParameterExpression.Name != "counts") throw new Exception("Root lost parameter.");
#endif
    var counts = new[] { 1, 2 };
    if (db.Set<Nest>().Count(x => counts.Contains(x.Id)) != 2) throw new Exception("Collection query failed.");
}
Console.WriteLine("PASS structural contract, entity/nested/null reads, parameter root and collection query");
Console.WriteLine("SQLite " + connection.ServerVersion);
#if LEGACY_HOOK_PROBE
// Compile-only: EF10 already rejects the entity-only contract. Never execute it.
IEntityMaterializerSource? retired = null;
Console.WriteLine(retired);
#endif
sealed class NestContext(DbContextOptions<NestContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Nest>().ComplexProperty(x => x.Location);
}
sealed class Nest { public int Id { get; set; } public Location Location { get; set; } = new(); public string? Note { get; set; } }
sealed class Location { public string Ridge { get; set; } = ""; }
// endsnippet:complete
