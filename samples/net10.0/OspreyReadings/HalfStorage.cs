// snippet:complete
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

static class HalfStorage
{
    public static void Run()
    {
        using var connection = new SqliteConnection("Data Source=:memory:"); connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "CREATE TABLE Gauges (Value REAL);"; command.ExecuteNonQuery();
        Half?[] values = [(Half)0, (Half)1.1, Half.MaxValue, Half.MinValue, Half.Epsilon, null];
        foreach (var value in values)
        {
            command.CommandText = "DELETE FROM Gauges; INSERT INTO Gauges VALUES ($value);";
            command.Parameters.Clear(); command.Parameters.AddWithValue("$value", value.HasValue ? (object)(double)value.Value : DBNull.Value); command.ExecuteNonQuery();
            command.CommandText = "SELECT Value, typeof(Value) FROM Gauges";
            using var reader = command.ExecuteReader(); reader.Read();
            Half? result = reader.IsDBNull(0) ? null : (Half)reader.GetDouble(0);
            if (result != value || reader.GetString(1) != (value.HasValue ? "real" : "null")) throw new Exception("Half round-trip");
            Console.WriteLine($"PASS Half direct {value}: {reader.GetString(1)}");
        }
        command.Parameters.Clear(); command.CommandText = "SELECT 100000.0";
        using (var reader = command.ExecuteReader()) { reader.Read(); if (!Half.IsPositiveInfinity((Half)reader.GetDouble(0))) throw new Exception("Expected range overflow to infinity"); }
        var options = new DbContextOptionsBuilder<Gauges>().UseSqlite(connection).Options;
        using (var generic = new GenericGauges(new DbContextOptionsBuilder<GenericGauges>().UseSqlite(connection).Options))
        {
            try { _ = generic.Model; throw new Exception("Expected generic Half converter rejection"); }
            catch (InvalidOperationException error) when (error.Message.Contains("could not be mapped", StringComparison.Ordinal))
            { Console.WriteLine("PASS EF10 generic Half converter rejected; explicit expressions required"); }
        }
        using (var db = new Gauges(options)) { db.Database.EnsureCreated(); }
        // EnsureCreated does not add tables to a database that already contains Gauges.
        command.CommandText = "CREATE TABLE Measurements (Id INTEGER PRIMARY KEY, Value REAL NULL)"; command.ExecuteNonQuery();
        using (var db = new Gauges(options)) { db.Measurements.AddRange(values.Select((v,i) => new Measurement { Id=i+1, Value=v })); db.SaveChanges(); }
        using (var db = new Gauges(options))
        {
            var actual = db.Measurements.AsNoTracking().OrderBy(x => x.Id).Select(x => x.Value).ToArray();
            if (!actual.SequenceEqual(values)) throw new Exception("EF Half round-trip");
            Console.WriteLine("PASS EF Half fresh-context nullable round-trip; REAL storage");
        }
        if ((Half)1.1 == (Half)1 || (double)(Half)1.1 == 1.1) throw new Exception("Precision control");
        Console.WriteLine("PASS Half rounds before storage; no two-byte SQLite storage promise");
    }
}
sealed class Gauges(DbContextOptions<Gauges> options) : DbContext(options)
{
    public DbSet<Measurement> Measurements => Set<Measurement>();
    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<Measurement>().Property(x => x.Value).HasConversion(v => (double)v!.Value, v => (Half)v);
}
sealed class Measurement { public int Id { get; set; } public Half? Value { get; set; } }
sealed class GenericGauges(DbContextOptions<GenericGauges> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<Measurement>().Property(x => x.Value).HasConversion<double>();
}
// endsnippet:complete


