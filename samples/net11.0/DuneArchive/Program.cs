// snippet:complete
#pragma warning disable EF9105 // Experimental SQL Server vector index/search APIs.
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;

var cs = Environment.GetEnvironmentVariable("PLAYBOOK_SQL_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Use an empty disposable playbook_ database through the isolated SQL route.");
await using var connection = new SqlConnection(cs);
await connection.OpenAsync();
if (!connection.Database.StartsWith("playbook_", StringComparison.Ordinal))
    throw new InvalidOperationException("An exclusively owned playbook_ database is required.");
Console.WriteLine($"SQL={connection.ServerVersion}; EF={typeof(DbContext).Assembly.GetName().Version}");
await using var db = new Archive(cs);
var ddl = db.Database.GenerateCreateScript();
Check(ddl.Contains("CREATE JSON INDEX", StringComparison.Ordinal), "modeled JSON index DDL");
await db.Database.EnsureCreatedAsync();
db.Cards.AddRange(new Card { Id = 1, Location = new() { Ridge = "West", Tray = 2 } },
    new Card { Id = 2, Location = new() { Ridge = "East", Tray = 7 } });
await db.SaveChangesAsync();
Check((await db.Cards.Where(x => x.Location.Ridge == "West").Select(x => x.Id).SingleAsync()) == 1, "JSON indexed path lookup");
await using (var command = connection.CreateCommand())
{
    command.CommandText = "SELECT COUNT(*) FROM sys.indexes WHERE object_id=OBJECT_ID('Cards') AND type_desc='JSON'";
    Check(Convert.ToInt32(await command.ExecuteScalarAsync()) == 1, "physical JSON index");
}

// The baseline table deliberately has no vector index so ordinary projection can
// be checked on the supplied engine independently of newer search syntax.
await db.Database.ExecuteSqlRawAsync("CREATE TABLE Echoes (Id int NOT NULL PRIMARY KEY CLUSTERED, Embedding vector(3) NOT NULL); INSERT Echoes VALUES (1,'[1,0,0]'),(2,'[0,1,0]'),(3,'[0,0,1]');");
await using var search = new EchoStore(cs);
var needle = new SqlVector<float>(new float[] { 1, 0, 0 });
// snippet:exact
var nearest = await search.Echoes.AsNoTracking()
    .OrderBy(x => EF.Functions.VectorDistance("cosine", x.Embedding, needle))
    .Select(x => x.Id).FirstAsync();
// endsnippet:exact
Check(nearest == 1, "exact distance nearest record");
// snippet:loading
var ordinary = search.Echoes.AsNoTracking().Where(x => x.Id == 1);
var entitySql = ordinary.ToQueryString();
Check(!entitySql.Contains("[e].[Embedding]", StringComparison.Ordinal), "default entity projection omits vector");
var entity = await ordinary.SingleAsync();
var persisted = await search.Echoes.Where(x => x.Id == entity.Id)
    .Select(x => x.Embedding).SingleAsync();
Check(persisted.Memory.Span.SequenceEqual(new float[] { 1, 0, 0 }), "explicit persisted vector projection");
// endsnippet:loading
await using var indexed = new IndexedEchoStore(cs);
var vectorDdl = indexed.Database.GenerateCreateScript();
Check(vectorDdl.Contains("CREATE VECTOR INDEX", StringComparison.Ordinal), "modeled vector index DDL");
// snippet:search
var exact = indexed.Echoes.VectorSearch(x => x.Embedding, needle, "cosine")
    .OrderBy(x => x.Distance).Take(2);
var approximate = indexed.Echoes.VectorSearch(x => x.Embedding, needle, "cosine")
    .OrderBy(x => x.Distance).Take(2).WithApproximate();
Console.WriteLine(exact.ToQueryString());
Console.WriteLine(approximate.ToQueryString());
// endsnippet:search
Check(approximate.ToQueryString().Contains("WITH APPROXIMATE", StringComparison.Ordinal), "approximate SQL generation");
// Opt in only on a server supporting the generated EF11 syntax. Local CU10
// rejects it; never turn the generated SQL check into execution evidence.
if (args.Contains("--execute-search", StringComparer.Ordinal))
{
    await search.Database.ExecuteSqlRawAsync("ALTER DATABASE SCOPED CONFIGURATION SET PREVIEW_FEATURES=ON;");
    await search.Database.ExecuteSqlRawAsync("INSERT Echoes SELECT value+3,'[0.9,0.1,0]' FROM GENERATE_SERIES(1,150);");
    await search.Database.ExecuteSqlRawAsync("CREATE VECTOR INDEX IX_Echoes_Embedding ON Echoes(Embedding) WITH (METRIC='cosine');");
    var exactRows = await exact.ToListAsync();
    var approximateRows = await approximate.ToListAsync();
    Check(exactRows.Count == 2 && exactRows[0].Value.Id == 1, "exact VectorSearch execution");
    Check(approximateRows.Count > 0 && approximateRows.Count <= 2 && approximateRows.All(x => double.IsFinite(x.Distance)), "bounded approximate results; recall is not guaranteed");
}
else Console.WriteLine("NOT TESTED: EF11 vector index application and exact/approximate VectorSearch execution; supplied SQL2025 lacks WITH APPROXIMATE.");
Console.WriteLine("PASS: DuneArchive supported behavior and SQL generation");

static void Check(bool valid, string claim) { if (!valid) throw new Exception(claim); Console.WriteLine("PASS: " + claim); }
sealed class Archive(string cs) : DbContext
{
    public DbSet<Card> Cards => Set<Card>();
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlServer(cs, s => s.UseCompatibilityLevel(170));
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Card>().Property(x => x.Id).ValueGeneratedNever();
        // snippet:json-model
        modelBuilder.Entity<Card>().ComplexProperty(x => x.Location, p => p.ToJson().HasColumnType("json"));
        modelBuilder.Entity<Card>().HasIndex(x => x.Location.Ridge);
        // endsnippet:json-model
    }
}
class EchoStore(string cs) : DbContext
{
    public DbSet<Echo> Echoes => Set<Echo>();
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlServer(cs, s => s.UseCompatibilityLevel(170));
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Echo>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<Echo>().Property(x => x.Embedding).HasColumnType("vector(3)");
    }
}
sealed class IndexedEchoStore(string cs) : EchoStore(cs)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // snippet:vector-model
        modelBuilder.Entity<Echo>().HasVectorIndex(x => x.Embedding).HasMetric("cosine");
        // endsnippet:vector-model
    }
}
sealed class Card { public int Id { get; set; } public Location Location { get; set; } = new(); }
sealed class Location { public string Ridge { get; set; } = ""; public int Tray { get; set; } }
sealed class Echo { public int Id { get; set; } public SqlVector<float> Embedding { get; set; } }
// endsnippet:complete
