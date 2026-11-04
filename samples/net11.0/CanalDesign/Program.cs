// snippet:complete
using CanalSamples;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

if (!args.Contains("--design-only"))
{
    foreach (var asynchronous in new[] { false, true })
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        using var context = new CanalContext(new DbContextOptionsBuilder<CanalContext>().UseSqlite(connection).Options);
        if (context.Database.GetMigrations().Any()) throw new InvalidOperationException("Expected an empty migration assembly.");
        try
        {
            #pragma warning disable CA1849 // Exercise both real sync and async APIs.
            if (asynchronous) await context.Database.MigrateAsync(); else context.Database.Migrate();
#pragma warning restore CA1849
            throw new InvalidOperationException("Expected an empty-assembly failure.");
        }
        catch (InvalidOperationException exception) when (exception.Message.Contains("MigrationsNotFound", StringComparison.Ordinal))
        {
            Console.WriteLine($"PASS real MigrationsNotFound exception (async={asynchronous}).");
        }

        // Another schema owner creates this table; the application only reads it.
        using var create = connection.CreateCommand();
        create.CommandText = "CREATE TABLE Moorings (Id INTEGER PRIMARY KEY, Label TEXT NOT NULL); INSERT INTO Moorings VALUES (7, 'West basin')";
        await create.ExecuteNonQueryAsync();
        if ((await context.Moorings.SingleAsync()).Label != "West basin") throw new InvalidOperationException("External schema read failed.");
        Console.WriteLine("PASS external schema owner: application reads without Migrate or EnsureCreated.");
    }
}
else Console.WriteLine("Canal design-time consumer; startup does not update a database.");

namespace CanalSamples
{
public sealed class Mooring { public int Id { get; set; } public string Label { get; set; } = ""; }
public sealed class CanalContext(DbContextOptions<CanalContext> options) : DbContext(options)
{
    public DbSet<Mooring> Moorings => Set<Mooring>();
}
public sealed class ArchiveContext(DbContextOptions<ArchiveContext> options) : DbContext(options)
{
    public DbSet<Mooring> Moorings => Set<Mooring>();
}
public sealed class CanalFactory : IDesignTimeDbContextFactory<CanalContext>
{
    public CanalContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<CanalContext>().UseSqlite("Data Source=canal-design.db").Options);
}
public sealed class ArchiveFactory : IDesignTimeDbContextFactory<ArchiveContext>
{
    public ArchiveContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<ArchiveContext>().UseSqlite("Data Source=canal-archive.db").Options);
}
}
// endsnippet:complete
