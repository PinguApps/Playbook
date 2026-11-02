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
// Generate the same model-difference operations used when scaffolding an initial migration.
var model = db.GetService<IDesignTimeModel>().Model.GetRelationalModel();
var differ = db.GetService<IMigrationsModelDiffer>();
var generator = db.GetService<IMigrationsSqlGenerator>();
var operations = differ.GetDifferences(null, model);
foreach (var command in generator.Generate(operations, db.GetService<IDesignTimeModel>().Model))
{
    Console.WriteLine(command.CommandText);
    // Each command is outside an explicit transaction, including full-text DDL.
    await db.Database.ExecuteSqlRawAsync(command.CommandText);
}
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
var lexical = db.Notes.ContainsTable<QuarryNote, int>("basalt", n => n.Text);
var ranked = await lexical.OrderByDescending(r => r.Rank).ThenBy(r => r.Key).ToArrayAsync();
if (!ranked.Select(r => r.Key).Order().SequenceEqual([1, 2]) || ranked.Any(r => r.Rank <= 0))
    throw new InvalidOperationException("Ranked text matching failed.");
var broad = await db.Notes.FreeTextTable<QuarryNote, int>("drainage", n => n.Text).ToArrayAsync();
if (!broad.Any(r => r.Key == 1)) throw new InvalidOperationException("Free-text table matching failed.");
// endsnippet:ranked
// snippet:hybrid
var needle = new SqlVector<float>(new float[] { 1, 0, 0 });
// A left join retains semantic matches without the literal word. Normalize the
// lexical score and cosine similarity separately; this is a weighted sum, not RRF.
var combined = from note in db.Notes
               join hit in lexical.Select(r => new { r.Key, Rank = (int?)r.Rank }) on note.Id equals hit.Key into hits
               from hit in hits.DefaultIfEmpty()
               select new
               {
                   note.Id,
                   Score = ((hit.Rank ?? 0) / 1000.0)
                       + (1.0 - EF.Functions.VectorDistance("cosine", note.Embedding, needle) / 2.0)
               };
var query = combined.OrderByDescending(r => r.Score).ThenBy(r => r.Id).Take(3);
Console.WriteLine(query.ToQueryString());
var results = await query.ToArrayAsync();
// endsnippet:hybrid
if (results[0].Id != 1 || !results.Any(r => r.Id == 3) || results.Any(r => r.Id == 4)
    || results.Any(r => !double.IsFinite(r.Score))) throw new InvalidOperationException("Hybrid ranking failed.");
Console.WriteLine("PASS: migrated full-text model; ContainsTable/FreeTextTable ranks; combined SQL ranking retains semantic-only match.");
foreach (var command in generator.Generate(differ.GetDifferences(model, null), db.GetService<IDesignTimeModel>().Model))
    await db.Database.ExecuteSqlRawAsync(command.CommandText);
Console.WriteLine("PASS: reverse migration removed table/catalog/index.");

sealed class SearchStore(DbContextOptions<SearchStore> options) : DbContext(options)
{
    public DbSet<QuarryNote> Notes => Set<QuarryNote>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // snippet:model
        modelBuilder.HasFullTextCatalog("QuarryWords");
        var notes = modelBuilder.Entity<QuarryNote>();
        notes.ToTable("QuarryNotes");
        notes.Property(n => n.Id).ValueGeneratedNever();
        notes.Property(n => n.Embedding).HasColumnType("vector(3)");
        notes.HasFullTextIndex(n => n.Text).UseKeyIndex("PK_QuarryNotes")
            .UseCatalog("QuarryWords").UseLanguage("Text", "English");
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

