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
var expectedCallback = true;
if (IgnoreDraft.Seen != expectedCallback) throw new Exception("Unexpected convention ordering behavior.");
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
sealed class IgnoreDraft : ITypeIgnoredConvention
{
    public static bool IgnoreAfterAdding { get; set; }
    public static bool Seen { get; private set; }
    public void ProcessTypeIgnored(IConventionModelBuilder modelBuilder, string name, Type? type, IConventionContext<string> context)
    {
        if (type == typeof(DraftNest)) Seen = true;
    }
}
sealed class Nest { public int Id { get; set; } }
sealed class DraftNest { public int Id { get; set; } }
// endsnippet:complete
