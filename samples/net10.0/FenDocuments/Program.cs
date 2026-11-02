// snippet:complete
using System.Runtime.InteropServices;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

var connectionString = Environment.GetEnvironmentVariable("PLAYBOOK_SQL_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Supply a private disposable SQL connection.");
await using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();
if (!connection.Database.StartsWith("playbook_", StringComparison.Ordinal))
    throw new InvalidOperationException("Use an empty, exclusively owned playbook_ database.");
Console.WriteLine($"Runtime: {RuntimeInformation.FrameworkDescription}; EF: {typeof(DbContext).Assembly.GetName().Version}; SQL: {connection.ServerVersion}");
await using (var command = connection.CreateCommand())
{
    command.CommandText = """
        ALTER DATABASE CURRENT SET COMPATIBILITY_LEVEL = 170;
        CREATE TABLE dbo.FieldNotes (Id int NOT NULL PRIMARY KEY, Document json NULL,
          Tags json NOT NULL, NullableTags json NOT NULL, Wanted nvarchar(max) NULL);
        INSERT dbo.FieldNotes VALUES
          (1,N'{"survey":{"code":"reed"},"depth":7}',N'["reed","pool"]',N'["reed",null]',NULL),
          (2,N'{"survey":{"code":null},"depth":9}',N'["bank"]',N'["bank"]',N'bank'),
          (3,N'{"survey":{"code":""},"depth":7}',N'[]',N'[]',NULL),
          (4,N'{"survey":{}}',N'["reed"]',N'[null]',NULL),
          (5,NULL,N'[]',N'[]',NULL);
        """;
    await command.ExecuteNonQueryAsync();
}
// snippet:configuration
var options = new DbContextOptionsBuilder<NotesContext>()
    .UseSqlServer(connectionString, sql => sql.UseCompatibilityLevel(170)).Options;
await using var db = new NotesContext(options);
// endsnippet:configuration
// snippet:presence
var present = db.Notes.Where(n => LegacyJson.PathExists(n.Document, "$.survey.code") == 1);
// endsnippet:presence
await Expect(present, [1, 2, 3], "JSON_PATH_EXISTS");
// snippet:containment
var depth = db.Notes.Where(n => LegacyJson.Contains(n.Document, 7, "$.depth", 0) == 1);
var wildcard = db.Notes.Where(n => LegacyJson.ContainsText(n.Document, "re%", "$.survey.code", 1) == 1);
var missing = db.Notes.Where(n => LegacyJson.Contains(n.Document, 7, "$.missing", 0) == null);
// endsnippet:containment
await Expect(depth, [1, 3], "JSON_CONTAINS");
await Expect(wildcard, [1], "JSON_CONTAINS");
await Expect(missing, [1, 2, 3, 4, 5], "JSON_CONTAINS");
// snippet:membership
var members = db.Notes.Where(n => n.Tags.Contains("reed"));
var nullable = db.Notes.Where(n => n.NullableTags.Contains(n.Wanted));
// endsnippet:membership
await Expect(members, [1, 4], "OPENJSON");
await Expect(nullable, [1, 2, 4], "OPENJSON");
Console.WriteLine("PASS: presence includes null/empty; containment/path/LIKE and nullable membership executed.");

async Task Expect(IQueryable<FieldNote> query, int[] expected, string translation)
{
    var sql = query.OrderBy(n => n.Id).Select(n => n.Id).ToQueryString();
    Console.WriteLine(sql);
    if (!sql.Contains(translation, StringComparison.Ordinal)) throw new InvalidOperationException("Unexpected translation: " + translation);
    var actual = await query.OrderBy(n => n.Id).Select(n => n.Id).ToArrayAsync();
    if (!actual.SequenceEqual(expected)) throw new InvalidOperationException($"Expected {string.Join(',', expected)}; received {string.Join(',', actual)}");
}

internal sealed class FieldNote
{
    public int Id { get; set; }
    public string? Document { get; set; }
    public List<string> Tags { get; set; } = [];
    public List<string?> NullableTags { get; set; } = [];
    public string? Wanted { get; set; }
}

internal sealed class NotesContext(DbContextOptions<NotesContext> options) : DbContext(options)
{
    public DbSet<FieldNote> Notes => Set<FieldNote>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDbFunction(typeof(LegacyJson).GetMethod(nameof(LegacyJson.PathExists))!).HasName("JSON_PATH_EXISTS").IsBuiltIn();
        modelBuilder.HasDbFunction(typeof(LegacyJson).GetMethod(nameof(LegacyJson.Contains))!).HasName("JSON_CONTAINS").IsBuiltIn();
        modelBuilder.HasDbFunction(typeof(LegacyJson).GetMethod(nameof(LegacyJson.ContainsText))!).HasName("JSON_CONTAINS").IsBuiltIn();
        modelBuilder.Entity<FieldNote>().ToTable("FieldNotes");
        modelBuilder.Entity<FieldNote>().Property(n => n.Document).HasColumnType("json");
        modelBuilder.Entity<FieldNote>().Property(n => n.Tags).HasColumnType("json");
        modelBuilder.Entity<FieldNote>().Property(n => n.NullableTags).HasColumnType("json");
    }
}
// These methods are SQL translation markers, never client implementations.
internal static class LegacyJson
{
    public static int? PathExists(string? document, string path) => throw new NotSupportedException();
    public static int? ContainsText(string? document, string value, string path, int mode) => throw new NotSupportedException();
    public static int? Contains(string? document, int value, string path, int mode = 0) => throw new NotSupportedException();
}
// endsnippet:complete
