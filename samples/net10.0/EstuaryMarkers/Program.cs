// snippet:complete
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EstuaryMarkers;

internal static class Program
{
    public static void Main()
    {
        using SqliteConnection connection = new("Data Source=:memory:");
        connection.Open();
        DbContextOptions<MarkerContext> options = new DbContextOptionsBuilder<MarkerContext>()
            .UseSqlite(connection).Options;

        using (MarkerContext context = new(options))
        {
            IProperty code = context.Model.FindEntityType(typeof(Marker))!
                .FindComplexProperty(nameof(Marker.Site))!.ComplexType
                .FindComplexProperty(nameof(SurveySite.Bearing))!.ComplexType
                .FindProperty(nameof(Bearing.Code))!;
            Require(code.GetMaxLength() == 12 && !code.IsNullable, "nested scalar facets");
            Require(code.GetColumnName() == "marker_code", "nested column name");
            context.Database.EnsureCreated();
            using SqliteCommand schema = connection.CreateCommand();
            schema.CommandText = "SELECT type FROM pragma_table_info('Markers') WHERE name = 'marker_code'";
            Require((string?)schema.ExecuteScalar() == "TEXT", "real nested scalar schema");
            context.Markers.Add(new Marker { Site = new() { Bearing = new() { Code = "EST-42" } } });
            context.SaveChanges();
        }

        using (MarkerContext context = new(options))
        {
            Marker marker = context.Markers.Single();
            Require(marker.Site.Bearing.Code == "EST-42", "fresh-context nested read");
            marker.Site.Bearing.Code = "EST-43";
            context.SaveChanges();
        }
        using (MarkerContext context = new(options))
        {
            Require(context.Markers.AsNoTracking().Single().Site.Bearing.Code == "EST-43", "nested update round-trip");
        }
        using SqliteCommand version = connection.CreateCommand();
        version.CommandText = "SELECT sqlite_version()";
        Console.WriteLine($"PASS: nested model facets, TEXT schema, insert/read/update across contexts; SQLite {version.ExecuteScalar()}; EF {typeof(DbContext).Assembly.GetName().Version}.");
    }

    private static void Require(bool condition, string claim)
    {
        if (!condition) { throw new InvalidOperationException(claim); }
    }
}

internal sealed class MarkerContext(DbContextOptions<MarkerContext> options) : DbContext(options)
{
    public DbSet<Marker> Markers => Set<Marker>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Marker>()
            .ComplexProperty(marker => marker.Site)
            .ComplexProperty(site => site.Bearing)
            .Property(bearing => bearing.Code)
            .HasColumnName("marker_code").HasMaxLength(12).IsRequired();
    }
}

internal sealed class Marker
{
    public int Id { get; set; }
    public SurveySite Site { get; set; } = new();
}

[ComplexType]
internal sealed class SurveySite
{
    public Bearing Bearing { get; set; } = new();
}

[ComplexType]
internal sealed class Bearing
{
    public string Code { get; set; } = "";
}
// endsnippet:complete
