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
    Require(!db.ChangeTracker.SelectStates(modified: true).Any(), "undetected mutation is Unchanged");
    Require(!db.ChangeTracker.SelectStates<Nest>(modified: true).Any(), "generic stale state");
    Require(scans == 0, "selection adds no scan");
    db.ChangeTracker.DetectChanges();
    Require(scans == 1, "one explicit scan");
    Require(db.ChangeTracker.SelectStates<Nest>(modified: true).Single().Entity == rows[0], "detected mutation selected");
    rows[0].Name = "Wren final";
    db.Add(new Nest { Name = "Robin", Count = 3 });
    db.Remove(rows[1]);
    db.Attach(new Nest { Id = 999, Name = "Unchanged control" });
    Require(!db.ChangeTracker.SelectStates().Any(), "non-generic defaults false");
    Require(!db.ChangeTracker.SelectStates<Nest>().Any(), "generic defaults false");
    foreach (var state in new[] { EntityState.Added, EntityState.Modified, EntityState.Deleted, EntityState.Unchanged })
    {
        var selected = db.ChangeTracker.SelectStates(added: state == EntityState.Added, modified: state == EntityState.Modified, deleted: state == EntityState.Deleted, unchanged: state == EntityState.Unchanged).ToList();
        var typed = db.ChangeTracker.SelectStates<Nest>(added: state == EntityState.Added, modified: state == EntityState.Modified, deleted: state == EntityState.Deleted, unchanged: state == EntityState.Unchanged).ToList();
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


        db.Entry(partial).State = EntityState.Modified;
        scalar.IsModified = false; // Clear after marking the entity modified.
        db.SaveChanges();
        Require(!sql.Text.Last(t => t.StartsWith("UPDATE", StringComparison.Ordinal)).Contains("\"Count\"", StringComparison.Ordinal), "UPDATE omits scalar");
        using var reload = new NestContext(options);
        Require(reload.Nests.Single(n => n.Id == 1).Count == 42, "unread value preserved");
        // Deliberately enable writing the default value.
        scalar.CurrentValue = 0;
        scalar.IsModified = true;
        db.SaveChanges();
        Require(sql.Text.Last(t => t.StartsWith("UPDATE", StringComparison.Ordinal)).Contains("\"Count\"", StringComparison.Ordinal), "re-enabled UPDATE includes scalar");
    }
    using (var db = new NestContext(options))
    {
        var loaded = db.Nests.Single(n => n.Id == 1);
        Require(loaded.Count == 0, "loaded default is real");
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
    services.AddPooledDbContextFactory<NestContext>(_ => { }, poolSize: 1); // EF10 requires a callback. Shared configuration still supplies the provider.
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




static class KnownStates
{
    public static IEnumerable<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry> SelectStates(this Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker tracker,
        bool added = false, bool modified = false, bool deleted = false, bool unchanged = false)
    {
        bool previous = tracker.AutoDetectChangesEnabled;
        try
        {
            tracker.AutoDetectChangesEnabled = false;
            return tracker.Entries().Where(e => e.State switch
            {
                EntityState.Added => added, EntityState.Modified => modified,
                EntityState.Deleted => deleted, EntityState.Unchanged => unchanged, _ => false
            }).ToList();
        }
        finally { tracker.AutoDetectChangesEnabled = previous; }
    }
    public static IEnumerable<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<T>> SelectStates<T>(this Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker tracker,
        bool added = false, bool modified = false, bool deleted = false, bool unchanged = false) where T : class
    {
        var selected = tracker.SelectStates(added, modified, deleted, unchanged);
        bool previous = tracker.AutoDetectChangesEnabled;
        try
        {
            tracker.AutoDetectChangesEnabled = false;
            var entities = selected.Select(e => e.Entity).ToHashSet(ReferenceEqualityComparer.Instance);
            return tracker.Entries<T>().Where(e => entities.Contains(e.Entity)).ToList();
        }
        finally { tracker.AutoDetectChangesEnabled = previous; }
    }
}
// endsnippet:complete

