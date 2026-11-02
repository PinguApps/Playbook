// snippet:complete
#pragma warning disable EF9105 // Experimental vector distance API; exact scan, no vector index.
using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

var cs = Environment.GetEnvironmentVariable("PLAYBOOK_SQL_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Use the isolated SQL Server route.");
var options = new DbContextOptionsBuilder<SearchStore>().UseSqlServer(cs, s => s.UseCompatibilityLevel(170)).Options;
await using var db = new SearchStore(options);
if (!db.Database.GetDbConnection().Database.StartsWith("playbook_", StringComparison.Ordinal))
    throw new InvalidOperationException("Use an exclusively owned disposable playbook_ database.");
await db.Database.ExecuteSqlRawAsync("ALTER DATABASE CURRENT SET COMPATIBILITY_LEVEL = 170;");
// Older migrations use explicit SQL outside a transaction.
await db.Database.ExecuteSqlRawAsync("CREATE TABLE QuarryNotes (Id int NOT NULL CONSTRAINT PK_QuarryNotes PRIMARY KEY, Text nvarchar(max) NOT NULL, Embedding vector(3) NOT NULL);");
await db.Database.ExecuteSqlRawAsync("CREATE FULLTEXT CATALOG QuarryWords;");
await db.Database.ExecuteSqlRawAsync("CREATE FULLTEXT INDEX ON QuarryNotes(Text LANGUAGE 1033) KEY INDEX PK_QuarryNotes ON QuarryWords;");
await db.Database.ExecuteSqlRawAsync("""
    INSERT QuarryNotes VALUES
      (1,N'Basalt drainage inspection', '[1,0,0]'),
      (2,N'Basalt wall repair', '[0,1,0]'),
      (3,N'Water channel survey', '[0.9,0.1,0]'),
      (4,N'Office stationery', '[-1,0,0]');
    """);
// Full-text population is asynchronous. Bound the readiness wait to 30 seconds.
for (var attempt = 0; ; attempt++)
{
    var count = await db.Database.SqlQuery<int>($"SELECT COUNT(*) AS [Value] FROM CONTAINSTABLE(QuarryNotes, Text, N'basalt')").SingleAsync();
    if (count == 2) break;
    if (attempt == 59) throw new InvalidOperationException("Full-text population did not become ready.");
    await Task.Delay(500);
}
// snippet:ranked
var term = "basalt";
var ranked = await db.Database.SqlQuery<TextHit>($"SELECT [KEY] AS Id, [RANK] AS Rank FROM CONTAINSTABLE(QuarryNotes, Text, {term})").OrderByDescending(r => r.Rank).ThenBy(r => r.Id).ToArrayAsync();
if (!ranked.Select(r => r.Id).Order().SequenceEqual([1, 2]) || ranked.Any(r => r.Rank <= 0)) throw new InvalidOperationException("Ranked text matching failed.");
var broad = await db.Database.SqlQuery<TextHit>($"SELECT [KEY] AS Id, [RANK] AS Rank FROM FREETEXTTABLE(QuarryNotes, Text, N'drainage')").ToArrayAsync();
if (!broad.Any(r => r.Id == 1)) throw new InvalidOperationException("Free-text matching failed.");
// endsnippet:ranked
// snippet:hybrid
var vector = "[1,0,0]";
var query = db.Database.SqlQuery<SearchHit>($"""
    SELECT n.Id, COALESCE(t.[RANK] / 1000.0, 0.0)
        + (1.0 - VECTOR_DISTANCE('cosine', n.Embedding, CAST({vector} AS vector(3))) / 2.0) AS Score
    FROM QuarryNotes AS n
    LEFT JOIN CONTAINSTABLE(QuarryNotes, Text, {term}) AS t ON n.Id = t.[KEY]
    """).OrderByDescending(r => r.Score).ThenBy(r => r.Id).Take(3);
Console.WriteLine(query.ToQueryString());
var results = await query.ToArrayAsync();
// endsnippet:hybrid
if (results[0].Id != 1 || !results.Any(r => r.Id == 3) || results.Any(r => r.Id == 4)
    || results.Any(r => !double.IsFinite(r.Score))) throw new InvalidOperationException("Hybrid ranking failed.");
Console.WriteLine("PASS: migrated full-text model; ContainsTable/FreeTextTable ranks; combined SQL ranking retains semantic-only match.");
await db.Database.ExecuteSqlRawAsync("DROP FULLTEXT INDEX ON QuarryNotes;");
await db.Database.ExecuteSqlRawAsync("DROP FULLTEXT CATALOG QuarryWords;");
await db.Database.ExecuteSqlRawAsync("DROP TABLE QuarryNotes;");
Console.WriteLine("PASS: reverse migration removed table/catalog/index.");

sealed class TextHit { public int Id { get; set; } public int Rank { get; set; } }
sealed class SearchHit { public int Id { get; set; } public double Score { get; set; } }
sealed class SearchStore(DbContextOptions<SearchStore> options) : DbContext(options)
{
    public DbSet<QuarryNote> Notes => Set<QuarryNote>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // snippet:model

        var notes = modelBuilder.Entity<QuarryNote>();
        notes.ToTable("QuarryNotes");
        notes.Property(n => n.Id).ValueGeneratedNever();
        notes.Property(n => n.Embedding).HasColumnType("vector(3)");

        // endsnippet:model
    }
}
sealed class QuarryNote
{
    public int Id { get; set; }
    public string Text { get; set; } = "";
    public SqlVector<float> Embedding { get; set; }
}
// endsnippet:complete


