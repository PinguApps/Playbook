// snippet:complete
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

var connection = Environment.GetEnvironmentVariable("PLAYBOOK_SQL_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Use the isolated SQL Server runner.");
var log = new Commands();
var transactionStarts = 0;
DbContextOptions<Alder> Sql(ParameterTranslationMode mode = ParameterTranslationMode.MultipleParameters)
    => new DbContextOptionsBuilder<Alder>().UseSqlServer(connection,
        sql => {
            sql.MaxBatchSize(1);
#pragma warning disable CS0618 // Deliberate old helper comparison.
            if (mode == ParameterTranslationMode.Constant) sql.TranslateParameterizedCollectionsToConstants();
            else if (mode == ParameterTranslationMode.Parameter) sql.TranslateParameterizedCollectionsToParameters();
            else sql.UseParameterizedCollectionMode(mode);
#pragma warning restore CS0618
        }).AddInterceptors(log).LogTo(_ => transactionStarts++, [RelationalEventId.TransactionStarted]).Options;
using (var db = new Alder(Sql()))
{
    db.Database.EnsureCreated();
    Console.WriteLine("SQL Server " + db.Database.SqlQueryRaw<string>("SELECT CAST(SERVERPROPERTY('ProductVersion') AS nvarchar(128)) AS [Value]").Single());
}
using (var oldBoolean = new Alder(Sql()))
{
    oldBoolean.Database.ExecuteSql($"DELETE FROM [Parcels]");
#pragma warning disable CS0618 // Measure the old boolean without setting its replacement enum.
    oldBoolean.Database.AutoTransactionsEnabled = true;
#pragma warning restore CS0618
    transactionStarts = 0;
    oldBoolean.Parcels.Add(new Parcel { Id = 98, Label = "Old boolean single" });
    oldBoolean.SaveChanges();
    Check(transactionStarts == 0, "old boolean true single-command save starts no transaction (WhenNeeded, not Always)");
}
foreach (var behavior in Enum.GetValues<AutoTransactionBehavior>())
{
    using (var reset = new Alder(Sql())) reset.Database.ExecuteSql($"DELETE FROM [Parcels]");
    using (var db = new Alder(Sql()))
    {
        #pragma warning disable CS0618 // Deliberately exercise the obsolete EF10 boolean contract.
        db.Database.AutoTransactionsEnabled = behavior != AutoTransactionBehavior.Never;
#pragma warning restore CS0618
        if (behavior == AutoTransactionBehavior.Always) db.Database.AutoTransactionBehavior = behavior;
        db.Parcels.AddRange(new Parcel { Id = 1, Label = "First" }, new Parcel { Id = 2, Label = "Reject" });
        try { db.SaveChanges(); throw new Exception("Expected constraint rejection"); }
        catch (DbUpdateException) { }
    }
    using (var read = new Alder(Sql()))
    {
        var ids = read.Parcels.Select(x => x.Id).ToArray();
        Check(ids.SequenceEqual(behavior == AutoTransactionBehavior.Never ? new[] { 1 } : Array.Empty<int>()), "rollback " + behavior);
    }
}
foreach (var behavior in Enum.GetValues<AutoTransactionBehavior>())
{
    using var db = new Alder(Sql());
    db.Database.ExecuteSql($"DELETE FROM [Parcels]");
    db.Database.AutoTransactionBehavior = behavior;
    transactionStarts = 0;
    db.Parcels.Add(new Parcel { Id = 99, Label = "Single" });
    db.SaveChanges();
    Check(transactionStarts == (behavior == AutoTransactionBehavior.Always ? 1 : 0), "single-command transaction " + behavior);
}
using (var seed = new Alder(Sql()))
{
    seed.Database.ExecuteSql($"DELETE FROM [Parcels]");
    seed.Parcels.AddRange(Enumerable.Range(1, 4).Select(i => new Parcel { Id = i, Label = "Alder " + i }));
    seed.SaveChanges();
}
foreach (var mode in Enum.GetValues<ParameterTranslationMode>())
{
    using var services = new ServiceCollection().AddEntityFrameworkSqlServer().BuildServiceProvider();
    using var db = new Alder(new DbContextOptionsBuilder<Alder>(Sql(mode)).UseInternalServiceProvider(services).Options);
    int[] requested = [1, 3];
    log.Clear();
    var ids = db.Parcels.Where(x => requested.Contains(x.Id)).OrderBy(x => x.Id).Select(x => x.Id).ToArray();
    Check(ids.SequenceEqual(requested), "Contains results " + mode);
    var command = log.Items.Single();
    Check(mode switch
    {
        ParameterTranslationMode.Constant => command.Parameters.Length == 0 && command.Sql.Contains("IN (1, 3)"),
        ParameterTranslationMode.Parameter => command.Parameters.Length == 1 && command.Sql.Contains("OPENJSON"),
        _ => command.Parameters.Length >= 2 && !command.Sql.Contains("OPENJSON")
    }, "executed SQL form " + mode);
    log.Clear();
    Check(db.Parcels.Where(x => EF.Constant(requested).Contains(x.Id)).Count() == 2, "constant override");
    Check(log.Items.Single().Parameters.Length == 0, "constant override SQL");
    log.Clear();
    Check(db.Parcels.Where(x => EF.Parameter(requested).Contains(x.Id)).Count() == 2, "parameter override");
    Check(log.Items.Single().Parameters.Length > 0, "parameter override SQL");
}
using var sqlite = new SqliteConnection("Data Source=:memory:");
sqlite.Open();
using (var db = new Alder(new DbContextOptionsBuilder<Alder>().UseSqlite(sqlite).AddInterceptors(log).Options))
{
    db.Database.EnsureCreated();
    var hostile = "otter'; DROP TABLE Parcels; --";
    db.Parcels.AddRange(new Parcel { Id = 1, Label = hostile }, new Parcel { Id = 2, Label = "Other" });
    db.SaveChanges();
    log.Clear();
    var rows = await db.Parcels.FromSqlInterpolated($"SELECT * FROM Parcels WHERE Label = {hostile}").ToListAsync();
    Check(rows.Count == 1 && rows[0].Id == 1, "metacharacter query value");
    Check(log.Items.Single().Parameters.Single() == hostile && !log.Items.Single().Sql.Contains(hostile), "query parameter separation");
    log.Clear();
    Check(await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE Parcels SET Label = {hostile} WHERE Id = {2}") == 1, "parameterized update");
    Check(log.Items.Single().Parameters.Contains(hostile) && !log.Items.Single().Sql.Contains(hostile), "update parameter separation");
    Check(await db.Parcels.CountAsync() == 2, "table survives");
    using var canceled = new CancellationTokenSource();
    canceled.Cancel();
    try { await db.Parcels.FromSqlInterpolated($"SELECT * FROM Parcels WHERE Label = {hostile}").ToListAsync(canceled.Token); throw new Exception("Expected cancellation"); }
    catch (OperationCanceledException) { Console.WriteLine("PASS: canceled query"); }
    try { await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE Parcels SET Label = {"canceled"}", canceled.Token); throw new Exception("Expected cancellation"); }
    catch (OperationCanceledException) { Console.WriteLine("PASS: canceled update"); }
    Check(!await db.Parcels.AnyAsync(x => x.Label == "canceled"), "canceled write absent");
    // Fixed authored SQL is an intentional negative control, never constructed from input.
    Check(await db.Parcels.FromSqlRaw("SELECT * FROM Parcels WHERE Label = 'missing' OR 1=1").CountAsync() == 2,
        "fixed unsafe SQL changes predicate semantics; renaming cannot sanitize SQL");
        Console.WriteLine("SQLite " + db.Database.SqlQueryRaw<string>("SELECT sqlite_version() AS Value").Single());
    Console.WriteLine("SQLite source " + db.Database.SqlQueryRaw<string>("SELECT sqlite_source_id() AS Value").Single());
    var native = Path.Combine(AppContext.BaseDirectory, "runtimes", "win-x64", "native", "e_sqlite3.dll");
    Console.WriteLine("Native SHA256 " + Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(native))));
}
static void Check(bool condition, string claim)
{
    if (!condition) throw new Exception(claim);
    Console.WriteLine("PASS: " + claim);
}
sealed class Parcel { public int Id { get; set; } public string Label { get; set; } = ""; }
sealed class Alder(DbContextOptions<Alder> options) : DbContext(options)
{
    public DbSet<Parcel> Parcels => Set<Parcel>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Parcel>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<Parcel>().ToTable("Parcels", cancellationToken => cancellationToken.HasCheckConstraint("CK_Label", "Label <> 'Reject'"));
    }
}
sealed class Commands : DbCommandInterceptor
{
    public List<(string Sql, string[] Parameters)> Items { get; } = [];
    public void Clear() => Items.Clear();
    void Capture(DbCommand command)
    {
        var values = command.Parameters.Cast<DbParameter>().Select(p => Convert.ToString(p.Value) ?? "NULL").ToArray();
        Items.Add((command.CommandText, values));
        Console.WriteLine(command.CommandText + " PARAMETERS " + string.Join(" | ", values));
    }
    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result) { Capture(command); return result; }
    public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result) { Capture(command); return result; }
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default) { Capture(command); return ValueTask.FromResult(result); }
    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default) { Capture(command); return ValueTask.FromResult(result); }
}
// endsnippet:complete
