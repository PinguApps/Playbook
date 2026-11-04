// snippet:complete
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

if (args.Length == 3 && args[0] == "inspect")
{
    using var connection = new SqliteConnection($"Data Source={args[1]};Mode=ReadOnly");
    connection.Open();
    using var command = connection.CreateCommand();
    command.CommandText = "SELECT name FROM sqlite_master WHERE name IN ('Berths','Beacons')";
    if ((string?)command.ExecuteScalar() != args[2]) throw new InvalidOperationException("Database target mismatch.");
    Console.WriteLine($"PASS {args[1]} owns {args[2]}.");
}
else
if (args.Length == 2 && args[0] == "execute")
{
    using var connection = new SqliteConnection("Data Source=:memory:");
    connection.Open();
    using var command = connection.CreateCommand();
    // Only execute the locally generated, reviewed migration artifact in this disposable database.
#pragma warning disable CA2100
    command.CommandText = File.ReadAllText(args[1]);
#pragma warning restore CA2100
    command.ExecuteNonQuery();
    command.CommandText = "SELECT count(*) FROM sqlite_master WHERE name IN ('Berths','Beacons')";
    if (Convert.ToInt64(command.ExecuteScalar()) != 1) throw new InvalidOperationException("Wrong script target.");
    Console.WriteLine("PASS executable SQL contains exactly one intended model table.");
}
else Console.WriteLine("Fleet tooling consumer: startup never migrates databases.");

namespace FleetSamples
{
    public sealed class Berth { public int Id { get; set; } public string Name { get; set; } = ""; }
    public sealed class Beacon { public int Id { get; set; } public string Code { get; set; } = ""; }
    public sealed class BerthContext(DbContextOptions<BerthContext> options) : DbContext(options)
    {
        public DbSet<Berth> Berths => Set<Berth>();
    }
    public sealed class BeaconContext(DbContextOptions<BeaconContext> options) : DbContext(options)
    {
        public DbSet<Beacon> Beacons => Set<Beacon>();
    }
    public sealed class BerthFactory : IDesignTimeDbContextFactory<BerthContext>
    {
        public BerthContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<BerthContext>().UseSqlite("Data Source=fleet-berths.db").Options);
    }
    public sealed class BeaconFactory : IDesignTimeDbContextFactory<BeaconContext>
    {
        public BeaconContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<BeaconContext>().UseSqlite("Data Source=fleet-beacons.db").Options);
    }
}
// endsnippet:complete


