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
    Check(db.Readings.Where(x => x.Levels != null && x.Levels.Exists(n => n > 5)), "list Exists", [1]);
    Check(db.Readings.Where(x => x.Signal != null && x.Signal.Any()), "binary Any", [1]);
    Check(db.Readings.Where(x => x.Levels != null && x.Levels.Any(n => n > 5)), "list Any alternative", [1]);
    Check(db.Readings.Where(x => x.Signal != null && x.Signal.Length > 0), "binary length alternative", [1]);
    if (provider == "sql")
    {
        Check(db.Readings.Where(x => x.Text != null && int.Parse(x.Text) > 5), "int Parse filter", [1]);
        log.Sql.Clear();
        var parsed = db.Readings.Where(x => x.Id == 1).Select(x => new
        {
            B = byte.Parse(x.Text!), S = short.Parse(x.Text!), I = int.Parse(x.Text!),
            L = long.Parse(x.Text!), D = decimal.Parse(x.Text!), F = float.Parse(x.Text!), R = double.Parse(x.Text!)
        }).Single();
        if (parsed.B != 12 || parsed.S != 12 || parsed.I != 12 || parsed.L != 12 || parsed.D != 12 || parsed.F != 12 || parsed.R != 12 || !log.Sql.Single().Contains("CAST", StringComparison.Ordinal)) throw new Exception("Parse overloads");
        Console.WriteLine("PASS seven single-string Parse overloads\n" + log.Sql.Single());
        var nullValue = db.Readings.Where(x => x.Id == 3).Select(x => (int?)int.Parse(x.Text!)).Single();
        if (nullValue != null) throw new Exception("SQL null propagation");
        if (double.Parse("1,5", System.Globalization.CultureInfo.GetCultureInfo("fr-FR")) != 1.5) throw new Exception("CLR culture control");
        Console.WriteLine("PASS SQL nullable projection; CLR French decimal control = 1.5");
        foreach (var text in new[] { "bad", "2147483648", "1,5" })
        {
            db.Readings.Add(new Reading { Id = 4, Text = text }); db.SaveChanges();
            try { _ = db.Readings.Where(x => x.Id == 4).Select(x => int.Parse(x.Text!)).ToArray(); throw new Exception("Expected SQL conversion error"); }
            catch (Microsoft.Data.SqlClient.SqlException e) { Console.WriteLine($"PASS SQL rejects {text}: {e.Number}"); }
            db.Readings.Remove(db.Readings.Single(x => x.Id == 4)); db.SaveChanges();
        }
    }
    void Check(IQueryable<Reading> query, string name, int[] expected)
    {
        log.Sql.Clear();
        var result = query.Select(x => x.Id).OrderBy(x => x).ToArray();
        if (!result.SequenceEqual(expected) || !log.Sql.Any(s => s.Contains("WHERE", StringComparison.Ordinal))) throw new Exception(name);
        Console.WriteLine($"PASS {provider} {name}: {string.Join(',', result)}\n{string.Join('\n', log.Sql)}");
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



