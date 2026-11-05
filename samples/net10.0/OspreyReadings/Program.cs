// snippet:complete
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

HalfStorage.Run();
using var sqlite = new SqliteConnection("Data Source=:memory:");
sqlite.Open();
Console.WriteLine("SQLite " + sqlite.ServerVersion);
foreach (var provider in new[] { "sqlite", "sql" })
{
    var log = new Commands();
    var builder = new DbContextOptionsBuilder<Survey>().AddInterceptors(log);
    if (provider == "sqlite") builder.UseSqlite(sqlite);
    else builder.UseSqlServer(Environment.GetEnvironmentVariable("PLAYBOOK_SQL_CONNECTION_STRING") ?? throw new Exception("Use isolated SQL runner"));
    using var db = new Survey(builder.Options);
    db.Database.EnsureCreated();
    db.Readings.AddRange(new Reading { Id = 1, Levels = [2, 8], Signal = [4], Text = "12" },
        new Reading { Id = 2, Levels = [], Signal = [], Text = "3" },
        new Reading { Id = 3, Levels = null, Signal = null, Text = null });
    db.SaveChanges();
    Reject(() => db.Readings.Where(x => x.Levels != null && x.Levels.Exists(n => n > 5)).ToArray(), "list Exists");
    Reject(() => db.Readings.Where(x => x.Signal != null && x.Signal.Any()).ToArray(), "binary Any");
    Reject(() => db.Readings.Where(x => x.Signal != null && x.Signal.Length > 0).ToArray(), "binary Length > 0");
    Check(db.Readings.Where(x => x.Levels != null && x.Levels.Any(n => n > 5)), "list Any alternative", [1]);
    Check(db.Readings.FromSqlRaw(provider == "sqlite" ? "SELECT * FROM Readings WHERE length(Signal) > 0" : "SELECT * FROM Readings WHERE DATALENGTH(Signal) > 0"), "binary SQL alternative", [1]);
    if (provider == "sql")
    {
        Reject(() => db.Readings.Where(x => x.Text != null && int.Parse(x.Text) > 5).ToArray(), "Parse filter");
        Check(db.Readings.FromSqlRaw("SELECT * FROM [Readings] WHERE TRY_CAST([Text] AS int) > 5"), "TRY_CAST alternative", [1]);
    }
    void Check(IQueryable<Reading> query, string name, int[] expected)
    {
        log.Sql.Clear();
        var result = query.Select(x => x.Id).OrderBy(x => x).ToArray();
        if (!result.SequenceEqual(expected) || !log.Sql.Any(s => s.Contains("WHERE", StringComparison.Ordinal))) throw new Exception(name);
        Console.WriteLine($"PASS {provider} {name}: {string.Join(',', result)}\n{string.Join('\n', log.Sql)}");
    }
    void Reject(Action action, string name)
    {
        log.Sql.Clear();
        try { action(); throw new Exception("Expected translation rejection: " + name); }
        catch (InvalidOperationException)
        {
            if (log.Sql.Count != 0) throw new Exception("Unexpected SQL");
            Console.WriteLine("PASS rejected before SQL: " + name);
        }
    }
}

sealed class Survey(DbContextOptions<Survey> options) : DbContext(options)
{
    public DbSet<Reading> Readings => Set<Reading>();
    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<Reading>().Property(x => x.Id).ValueGeneratedNever();
}
sealed class Reading
{
    public int Id { get; set; }
    public List<int>? Levels { get; set; }
    public byte[]? Signal { get; set; }
    public string? Text { get; set; }
}
sealed class Commands : DbCommandInterceptor
{
    public List<string> Sql { get; } = [];
    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    { Sql.Add(command.CommandText); return result; }
}
// endsnippet:complete




