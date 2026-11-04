// snippet:complete
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

var connection = Environment.GetEnvironmentVariable("PLAYBOOK_SQL_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Use the approved SqlServer.ps1 Run route.");
foreach (var legacy in new[] { true, false })
{
    var builder = new DbContextOptionsBuilder<DispatchContext>();
#if NET10_0
#pragma warning disable CS0618 // Deliberate before-upgrade comparison only.
    if (legacy) builder.UseSqlServer(connection, sql => sql.UseAzureSqlDefaults());
    else
#endif
        builder.UseAzureSql(connection);
    await using var context = new DispatchContext(builder.Options);
    var strategy = context.Database.CreateExecutionStrategy();
    if (strategy is not ExecutionStrategy retry || !retry.RetriesOnFailure
        || retry.MaxRetryCount != 6 || retry.MaxRetryDelay != TimeSpan.FromSeconds(30))
        throw new InvalidOperationException("Retry defaults changed.");
    await context.Database.EnsureCreatedAsync();
    var id = legacy ? 1 : 2;
    context.Add(new Dispatch { Id = id, Destination = "Gull Island" });
    await context.SaveChangesAsync();
    await using var fresh = new DispatchContext(builder.Options);
    if ((await fresh.Set<Dispatch>().SingleAsync(x => x.Id == id)).Destination != "Gull Island")
        throw new InvalidOperationException("Round-trip failed.");
    var compatibility = await fresh.Database.SqlQueryRaw<int>("SELECT CAST(compatibility_level AS int) AS [Value] FROM sys.databases WHERE name = DB_NAME()").SingleAsync();
    if (compatibility != 170) throw new InvalidOperationException("Expected server compatibility 170.");
    Console.WriteLine($"PASS SQL legacy={legacy}; {context.Database.ProviderName}; server compatibility={compatibility}; retries={retry.MaxRetryCount}; delay={retry.MaxRetryDelay}; insert/fresh query");
}
sealed class DispatchContext(DbContextOptions<DispatchContext> options) : DbContext(options) { public DbSet<Dispatch> Dispatches => Set<Dispatch>(); protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<Dispatch>().Property(x => x.Id).ValueGeneratedNever(); }
sealed class Dispatch { public int Id { get; set; } public string Destination { get; set; } = ""; }
// endsnippet:complete



