// snippet:complete
using System.Data.Common;
using System.Runtime.InteropServices;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

var connectionString = Environment.GetEnvironmentVariable("PLAYBOOK_SQL_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Supply an isolated disposable SQL database through PLAYBOOK_SQL_CONNECTION_STRING.");
await using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();
if (!connection.Database.StartsWith("playbook_", StringComparison.Ordinal))
    throw new InvalidOperationException("This destructive sample requires its own playbook_ database.");
Console.WriteLine($"Runtime: {RuntimeInformation.FrameworkDescription}; EF: {typeof(DbContext).Assembly.GetName().Version}; driver: {typeof(SqlConnection).Assembly.GetName().Version}; SQL: {connection.ServerVersion}");
await Sql("CREATE TABLE dbo.DispatchLoads (Id int NOT NULL PRIMARY KEY, Label nvarchar(40) NOT NULL, Crates int NOT NULL, Capacity int NOT NULL); INSERT dbo.DispatchLoads VALUES (11,N'Fern',4,7),(12,N'Moss',4,9),(21,N'Reed',9,6),(22,N'Heather',9,8);");

foreach (var level in new[] { 150, 170 })
{
    // Only this owned disposable database changes; provider configuration below issues no ALTER.
    await Sql($"ALTER DATABASE CURRENT SET COMPATIBILITY_LEVEL = {level}");
    var before = await Compatibility();
    var capture = new Commands();
    // snippet:configuration
    var options = new DbContextOptionsBuilder<DispatchContext>()
        .UseSqlServer(connectionString, sql => sql.UseCompatibilityLevel(150))
        .AddInterceptors(capture).Options;
    // endsnippet:configuration
    await using var context = new DispatchContext(options);
    var query = context.Loads.AsNoTracking();
    capture.Items.Clear();
    var requested = 3;
    var ceiling = 2;
    var allocated = await query.OrderBy(x => x.Id).Take(requested).Take(ceiling)
        .Select(x => x.Id).ToArrayAsync();
    Check(allocated.SequenceEqual(new[] { 11, 12 }), "Actual capped load selection");
    Check(!capture.Items.Single().Sql.Contains("LEAST", StringComparison.Ordinal) && capture.Items.Single().Sql.Contains("TOP", StringComparison.Ordinal), "Level150 fallback SQL");
    Console.WriteLine($"Provider150 on database{level}: {capture.Items.Single().Sql}");
    var defaults = new DbContextOptionsBuilder<DispatchContext>().UseSqlServer(connectionString).AddInterceptors(capture).Options;
    await using (var defaultContext = new DispatchContext(defaults))
    {
        capture.Items.Clear();
        var actual = await defaultContext.Loads.OrderBy(x => x.Id).Take(requested).Take(ceiling)
            .Select(x => x.Id).ToArrayAsync();
        Check(actual.SequenceEqual(allocated), "Default provider allocation results");
        Check(!capture.Items.Single().Sql.Contains("LEAST", StringComparison.Ordinal), "Measured default translation");
        Console.WriteLine($"Default provider on database{level}: {capture.Items.Single().Sql}");
    }
    Check(await Compatibility() == before, "Provider configuration/query did not upgrade database");

    capture.Items.Clear();
    // snippet:extrema
    var smallest = query.OrderBy(x => x.Crates).ThenBy(x => x.Id).FirstOrDefault();
    var largest = query.OrderByDescending(x => x.Crates).ThenByDescending(x => x.Id).FirstOrDefault();
    var smallestAsync = await query.OrderBy(x => x.Crates).ThenBy(x => x.Id).FirstOrDefaultAsync();
    var largestAsync = await query.OrderByDescending(x => x.Crates).ThenByDescending(x => x.Id).FirstOrDefaultAsync();
    // endsnippet:extrema
    Check(smallest?.Id == 11 && largest?.Id == 22, "Sync ties: lowest Id for minimum, highest Id for maximum");
    Check(smallestAsync?.Id == 11 && largestAsync?.Id == 22, "Async ties follow the same declared rule");
    Check(capture.Items.Count == 4 && capture.Items.Count(x => x.Async) == 2, "Four real commands, two async");
    foreach (var command in capture.Items)
    {
        Check(command.Sql.Contains("TOP(1)", StringComparison.Ordinal) && command.Sql.Contains("ORDER BY", StringComparison.Ordinal), "Server-side row selection");
        Check(command.Sql.Contains("[Crates]", StringComparison.Ordinal) && command.Sql.Contains("[Id]", StringComparison.Ordinal), "Explicit composite ordering");
        Console.WriteLine($"Extrema async={command.Async}: {command.Sql}");
    }
    Check(smallest!.Label == "Fern" && largest!.Label == "Heather", "Whole record materialization");
    var empty = query.Where(x => x.Id < 0);
    Check(empty.OrderBy(x => x.Crates).FirstOrDefault() is null, "Empty sync entity query returns null");
    Check(await empty.OrderByDescending(x => x.Crates).FirstOrDefaultAsync() is null, "Empty async entity query returns null");
    try { _ = query.MinBy(x => x.Crates); throw new Exception("Expected EF10 translation rejection"); }
    catch (InvalidOperationException error) when (error.Message.Contains("could not be translated", StringComparison.Ordinal))
    { Console.WriteLine("PASS: EF10 direct MinBy translation is unavailable; ordered alternative executes."); }
    Console.WriteLine($"PASS: database compatibility {level} unchanged; provider150/default SQL, whole records, sync/async deterministic ties and empty results.");
}

async Task Sql(string sql)
{
    // Only the fixed DDL and integer levels150/170 above reach this helper; no user SQL.
#pragma warning disable CA2100
    await using var command = new SqlCommand(sql, connection);
#pragma warning restore CA2100
    await command.ExecuteNonQueryAsync();
}
async Task<int> Compatibility()
{
    await using var command = new SqlCommand("SELECT compatibility_level FROM sys.databases WHERE name = DB_NAME()", connection);
    return Convert.ToInt32(await command.ExecuteScalarAsync());
}
static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}
sealed class DispatchContext(DbContextOptions<DispatchContext> options) : DbContext(options)
{
    public DbSet<DispatchLoad> Loads => Set<DispatchLoad>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.Entity<DispatchLoad>().ToTable("DispatchLoads");
}
sealed class DispatchLoad
{
    public int Id { get; set; }
    public required string Label { get; set; }
    public int Crates { get; set; }
    public int Capacity { get; set; }
}
sealed class Commands : DbCommandInterceptor
{
    public List<(string Sql, bool Async)> Items { get; } = [];
    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        Items.Add((command.CommandText, false));
        return result;
    }
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    {
        Items.Add((command.CommandText, true));
        return ValueTask.FromResult(result);
    }
}
// endsnippet:complete
