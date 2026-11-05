// snippet:complete
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

IgnoreDraft.IgnoreAfterAdding = args.Contains("--after-added");
using var connection = new SqliteConnection("Data Source=:memory:");
connection.Open();
Console.WriteLine("SQLite " + connection.ServerVersion);
using var db = new Rookery(new DbContextOptionsBuilder<Rookery>().UseSqlite(connection).Options);
db.Database.EnsureCreated();
if (db.Model.FindEntityType(typeof(DraftNest)) != null || db.Model.FindEntityType(typeof(Nest)) == null)
    throw new Exception("Ignored/included type mismatch.");
db.Add(new Nest { Id = 1 });
db.SaveChanges();
if (db.Set<Nest>().AsNoTracking().Single().Id != 1) throw new Exception("Persistence failed.");
using var command = connection.CreateCommand();
command.CommandText = "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='DraftNest'";
if (Convert.ToInt32(command.ExecuteScalar()) != 0) throw new Exception("Ignored table persisted.");
#if LEGACY_CALLBACK
Console.WriteLine("CONTROL LEGACY_CALLBACK: obsolete interface compiled and executed");
var expectedCallback = false;
#else
var expectedCallback = true;
#endif
if (IgnoreDraft.Seen != expectedCallback) throw new Exception($"Unexpected ordering: after={IgnoreDraft.IgnoreAfterAdding}, seen={IgnoreDraft.Seen}, expected={expectedCallback}");
Console.WriteLine("PASS ignored callback, included/ignored model and SQLite schema/persistence");

sealed class Rookery(DbContextOptions<Rookery> options) : DbContext(options)
{
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        => configurationBuilder.Conventions.Add(_ => new IgnoreDraft());
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Nest>();

        if (IgnoreDraft.IgnoreAfterAdding) modelBuilder.Entity<DraftNest>();
        modelBuilder.Ignore<DraftNest>();
    }
}
#pragma warning disable CS0618 // Historical callback comparison.
#if LEGACY_CALLBACK
sealed class IgnoreDraft : IEntityTypeIgnoredConvention
#else
sealed class IgnoreDraft : ITypeIgnoredConvention
#endif
{
    public static bool IgnoreAfterAdding { get; set; }
    public static bool Seen { get; private set; }
#if LEGACY_CALLBACK
    public void ProcessEntityTypeIgnored(
#else
    public void ProcessTypeIgnored(
#endif
IConventionModelBuilder modelBuilder, string name, Type? type, IConventionContext<string> context)
    {
        if (type == typeof(DraftNest)) Seen = true;
    }
}
#pragma warning restore CS0618
sealed class Nest { public int Id { get; set; } }
sealed class DraftNest { public int Id { get; set; } }
// endsnippet:complete
