// snippet:complete
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;

using SqliteConnection connection = new("Data Source=:memory:");
connection.Open();
var sql = new Commands();
var options = new DbContextOptionsBuilder<NestContext>().UseSqlite(connection).AddInterceptors(sql).Options;
using (var db = new NestContext(options))
{
    db.Database.EnsureCreated();
    db.AddRange(new Nest { Name = "Wren", Count = 42 }, new Nest { Name = "Finch", Count = 7 });
    db.SaveChanges();
}
CheckStates();
CheckScalars();
CheckPool();
using var version = connection.CreateCommand();
version.CommandText = "SELECT sqlite_version()";
Console.WriteLine($"PASS WrenTracking; EF {typeof(DbContext).Assembly.GetName().Version}; SQLite {version.ExecuteScalar()}; source {SQLitePCL.raw.sqlite3_sourceid().utf8_to_string()}");

void Require(bool condition, string claim)
{
    if (!condition) throw new InvalidOperationException(claim);
}
void CheckStates()
{
    int scans = 0;
    var trackedOptions = new DbContextOptionsBuilder<NestContext>(options).LogTo(_ => scans++, new[] { CoreEventId.DetectChangesStarting }).Options;
    using var db = new NestContext(trackedOptions);
    var rows = db.Nests.OrderBy(n => n.Id).ToList();
    rows[0].Name = "Wren revised";
    Require(!db.ChangeTracker.GetEntriesForState(modified: true).Any(), "undetected mutation is Unchanged");
    Require(!db.ChangeTracker.GetEntriesForState<Nest>(modified: true).Any(), "generic stale state");
    Require(scans == 0, "selection adds no scan");
    db.ChangeTracker.DetectChanges();
    Require(scans == 1, "one explicit scan");
    Require(db.ChangeTracker.GetEntriesForState<Nest>(modified: true).Single().Entity == rows[0], "detected mutation selected");
    rows[0].Name = "Wren final";
    db.Add(new Nest { Name = "Robin", Count = 3 });
    db.Remove(rows[1]);
    db.Attach(new Nest { Id = 999, Name = "Unchanged control" });
    Require(!db.ChangeTracker.GetEntriesForState().Any(), "non-generic defaults false");
    Require(!db.ChangeTracker.GetEntriesForState<Nest>().Any(), "generic defaults false");
    foreach (var state in new[] { EntityState.Added, EntityState.Modified, EntityState.Deleted, EntityState.Unchanged })
    {
        var selected = db.ChangeTracker.GetEntriesForState(added: state == EntityState.Added, modified: state == EntityState.Modified, deleted: state == EntityState.Deleted, unchanged: state == EntityState.Unchanged).ToList();
        var typed = db.ChangeTracker.GetEntriesForState<Nest>(added: state == EntityState.Added, modified: state == EntityState.Modified, deleted: state == EntityState.Deleted, unchanged: state == EntityState.Unchanged).ToList();
        Require(selected.Count == 1 && typed.Count == 1 && selected[0].State == state && typed[0].State == state, "each flag selects only its state");
    }
    Require(scans == 1, "flags add no scans");
    db.ChangeTracker.Entries().ToList();
    Require(scans == 2, "Entries adds a scan");
    db.SaveChanges();
    using var reload = new NestContext(options);
    Require(reload.Nests.OrderBy(n => n.Id).Select(n => n.Name).AsEnumerable().SequenceEqual(new List<string> { "Wren final", "Robin" }), "insert/update/delete reload");
    Console.WriteLine("PASS states: both overloads, stale/detected state, four flags/defaults, scan counts and persistence");
}
void CheckScalars()
{
    using (var db = new NestContext(options))
    {
        sql.Text.Clear();
        var row = db.Nests.Where(n => n.Id == 1).Select(n => new { n.Id, n.Name }).Single();
        Require(!sql.Text.Last().Contains("\"Count\"", StringComparison.Ordinal), "SELECT omits scalar");
        var partial = new Nest { Id = row.Id, Name = row.Name + " edit" };
        db.Attach(partial);
        var scalar = db.Entry(partial).Property(n => n.Count);
        Require(scalar.IsLoaded, "attach does not infer partial load");
        scalar.IsLoaded = false;
        db.Entry(partial).State = EntityState.Modified;
        db.SaveChanges();
        Require(!sql.Text.Last(t => t.StartsWith("UPDATE", StringComparison.Ordinal)).Contains("\"Count\"", StringComparison.Ordinal), "UPDATE omits scalar");
        using var reload = new NestContext(options);
        Require(reload.Nests.Single(n => n.Id == 1).Count == 42, "unread value preserved");
        int commandsBeforeEnable = sql.Text.Count;
        scalar.IsLoaded = true; // Does not fetch data.
        Require(scalar.CurrentValue == 0 && sql.Text.Count == commandsBeforeEnable, "re-enable does not fetch or change the value"); // Deliberately write the default value.
        scalar.CurrentValue = 0;
        scalar.IsModified = true;
        db.SaveChanges();
        Require(sql.Text.Last(t => t.StartsWith("UPDATE", StringComparison.Ordinal)).Contains("\"Count\"", StringComparison.Ordinal), "re-enabled UPDATE includes scalar");
    }
    using (var db = new NestContext(options))
    {
        var loaded = db.Nests.Single(n => n.Id == 1);
        Require(loaded.Count == 0 && db.Entry(loaded).Property(n => n.Count).IsLoaded, "loaded default is real");
        loaded.Count = 8;
        db.SaveChanges();
    }
    using (var reload = new NestContext(options)) Require(reload.Nests.Single(n => n.Id == 1).Count == 8, "loaded write persists");
    Console.WriteLine("PASS scalars: partial SELECT, unloaded UPDATE omission, reload, re-enable/default write and loaded update");
}
void CheckPool()
{
    ServiceCollection services = new();
    services.ConfigureDbContext<NestContext>(o => o.UseSqlite(connection).EnableDetailedErrors(), ServiceLifetime.Singleton);
    services.AddPooledDbContextFactory<NestContext>(poolSize: 1);
    using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    var factory = provider.GetRequiredService<IDbContextFactory<NestContext>>();
    NestContext first;
    using (first = factory.CreateDbContext())
    {
        Require(first.Database.GetDbConnection() == connection, "shared configuration consumed");
        first.RequestLabel = "request A";
        first.Nests.Single(n => n.Id == 1).Name = "unsaved request A";
    }
    using (var next = factory.CreateDbContext())
    {
        Require(ReferenceEquals(first, next), "disposed instance reused");
        Require(!next.ChangeTracker.Entries().Any() && next.RequestLabel.Length == 0, "tracking and explicit application reset");
        Require(next.Nests.Single(n => n.Id == 1).Name != "unsaved request A", "unsaved mutation did not leak");
        next.RequestLabel = "request B";
        next.Nests.Single(n => n.Id == 1).Name = "pooled save";
        next.SaveChanges();
    }
    using var reload = new NestContext(options);
    Require(reload.Nests.Single(n => n.Id == 1).Name == "pooled save", "pooled save persisted");
    Console.WriteLine("PASS pool: shared options, disposal/reuse, tracking and application reset, save/reload");
}
sealed class NestContext(DbContextOptions<NestContext> options) : DbContext(options)
{
    public DbSet<Nest> Nests => Set<Nest>();
    public string RequestLabel { get; set; } = "";
    public override void Dispose() { RequestLabel = ""; base.Dispose(); }
    public override ValueTask DisposeAsync() { RequestLabel = ""; return base.DisposeAsync(); }
}
sealed class Nest
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Count { get; set; }
}
sealed class Commands : DbCommandInterceptor
{
    public List<string> Text { get; } = [];
    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        Text.Add(command.CommandText);
        Console.WriteLine(command.CommandText);
        return result;
    }
}
// endsnippet:complete
