// snippet:complete
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

var connection = Environment.GetEnvironmentVariable("PLAYBOOK_SQL_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Use eng/SqlServer.ps1 with a disposable database.");
var options = new DbContextOptionsBuilder().UseSqlServer(connection).Options;
foreach (var mode in new[] { "Required", "Omitted", "Unconstrained" })
{
    using RelationshipContext db = mode switch
    {
        "Required" => new RequiredContext(options),
        "Omitted" => new OmittedContext(options),
        _ => new UnconstrainedContext(options)
    };
    var script = db.Database.GenerateCreateScript();
    var generatedForeignKey = script.Contains("FOREIGN KEY", StringComparison.Ordinal);
#if NET11_0
    if (generatedForeignKey != (mode == "Required")) throw new Exception("Unexpected generated constraint.");
#else
    if (!generatedForeignKey) throw new Exception("Expected older required constraint before explicit omission.");
#endif
    // The required control deliberately removes enforcement to expose its inner join.
    // EF10 also uses this explicit schema-authoring alternative for all three modes.
    script = Regex.Replace(script, @",\s*CONSTRAINT \[FK_[^\]]+\] FOREIGN KEY[^\r\n]+", "");
    foreach (var batch in Regex.Split(script, @"^GO\s*$", RegexOptions.Multiline))
        if (!string.IsNullOrWhiteSpace(batch)) db.Database.ExecuteSqlRaw(batch);
    // Identifiers come only from the three authored modes above, never from input.
#pragma warning disable EF1003
    db.Database.ExecuteSqlRaw($"INSERT INTO [{mode}Depots] (Id, Name) VALUES (1, N'North'); " +
        $"INSERT INTO [{mode}Crates] (Id, DepotId) VALUES (1, 1), (2, 999)");
#pragma warning restore EF1003
    var query = db.Set<Crate>().OrderBy(x => x.Id).Select(x => new { x.Id, Depot = x.Depot.Name });
    Console.WriteLine(mode + " " + query.ToQueryString());
    var rows = query.ToArray();
#if NET11_0
    var expected = mode == "Unconstrained" ? 2 : 1;
#else
    var expected = 1;
#endif
    if (rows.Length != expected || rows[0].Depot != "North")
        throw new Exception("Navigation join changed expected rows.");
#if NET11_0
    if (expected == 2 && rows[1].Depot is not null) throw new Exception("Missing principal should project null.");
#endif
    var fallback = (from crate in db.Set<Crate>()
                    join depot in db.Set<Depot>() on crate.DepotId equals depot.Id into matches
                    from depot in matches.DefaultIfEmpty()
                    orderby crate.Id
                    select new { crate.Id, Name = depot == null ? null : depot.Name }).ToArray();
    if (fallback.Length != 2 || fallback[1].Name is not null) throw new Exception("Explicit left join lost orphan.");
    var table = mode + "Crates";
    var fks = db.Database.SqlQuery<int>($"SELECT COUNT(*) AS [Value] FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID({table})").Single();
    var indexes = db.Database.SqlQuery<int>($"SELECT COUNT(*) AS [Value] FROM sys.indexes WHERE object_id = OBJECT_ID({table}) AND name LIKE 'IX_%'").Single();
    if (fks != 0 || indexes != 1) throw new Exception("Expected absent constraint and retained relationship index.");
    Console.WriteLine($"PASS {mode}: generatedFK={generatedForeignKey}, actualFK={fks}, indexes={indexes}, navigationRows={rows.Length}, explicitLeftRows={fallback.Length}");
}
abstract class RelationshipContext(DbContextOptions options) : DbContext(options)
{
    protected abstract string Mode { get; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Depot>().ToTable(Mode + "Depots");
        modelBuilder.Entity<Crate>().ToTable(Mode + "Crates");
        modelBuilder.Entity<Depot>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<Crate>().Property(x => x.Id).ValueGeneratedNever();
        var relationship = modelBuilder.Entity<Crate>().HasOne(x => x.Depot).WithMany().HasForeignKey(x => x.DepotId).IsRequired();
#if NET11_0
        if (Mode == "Omitted") relationship.ExcludeForeignKeyFromMigrations();
        if (Mode == "Unconstrained") relationship.IsConstrained(false);
#endif
    }
}
sealed class RequiredContext(DbContextOptions options) : RelationshipContext(options) { protected override string Mode => "Required"; }
sealed class OmittedContext(DbContextOptions options) : RelationshipContext(options) { protected override string Mode => "Omitted"; }
sealed class UnconstrainedContext(DbContextOptions options) : RelationshipContext(options) { protected override string Mode => "Unconstrained"; }
sealed class Depot { public int Id { get; set; } public string Name { get; set; } = ""; }
sealed class Crate { public int Id { get; set; } public int DepotId { get; set; } public Depot Depot { get; set; } = null!; }
// endsnippet:complete
