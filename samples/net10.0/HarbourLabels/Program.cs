// snippet:complete
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

await using var connection = new SqliteConnection("Data Source=:memory:");
await connection.OpenAsync();
await using var db = new HarbourContext(new DbContextOptionsBuilder<HarbourContext>().UseSqlite(connection).Options);
await db.Database.EnsureCreatedAsync();
db.Labels.AddRange(new Label { Id = 1, Harbour = "West", Position = 2, Text = "Buoy" },
    new Label { Id = 2, Harbour = "West", Position = 1, Text = "Anchor" },
    new Label { Id = 3, Harbour = "East", Position = 1, Text = "Lamp" },
    new Label { Id = 4, Harbour = "West", Position = 2, Text = "Chain" });
await db.SaveChangesAsync();
using var versionCommand = connection.CreateCommand();
versionCommand.CommandText = "SELECT sqlite_version()";
var engine = (string)(await versionCommand.ExecuteScalarAsync())!;
if (Version.Parse(engine) < new Version(3,44)) throw new InvalidOperationException("SQLite 3.44+ required.");
// snippet:query
var rows = await db.Labels.AsNoTracking().ToArrayAsync();
var result = rows.GroupBy(x => x.Harbour).ToDictionary(g => g.Key,
    g => string.Join(" / ", g.OrderBy(x => x.Position).ThenBy(x => x.Id).Select(x => x.Text)));
// endsnippet:query

if (result.Count != 2 || result["West"] != "Anchor / Buoy / Chain" || result["East"] != "Lamp")
    throw new InvalidOperationException("Ordered summaries differ.");
Console.WriteLine($"PASS ordered harbour summaries; SQLite {engine}; EF {typeof(DbContext).Assembly.GetName().Version}; runtime {Environment.Version}");
sealed class HarbourContext(DbContextOptions<HarbourContext> options) : DbContext(options) { public DbSet<Label> Labels => Set<Label>(); }
sealed class Label { public int Id { get; set; } public required string Harbour { get; set; } public int Position { get; set; } public required string Text { get; set; } }
// endsnippet:complete
