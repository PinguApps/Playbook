// snippet:complete
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using QuarrySamples;

using var context = new QuarryContext();
if (args.Contains("--inspect"))
{
    var entity = context.Model.FindEntityType(typeof(Crate))!;
    if (entity.GetForeignKeys().Count() != 1 || !entity.GetIndexes().Any(i => i.Properties.Single().Name == "DepotId"))
        throw new InvalidOperationException("Relationship or index missing from model.");
    var connection = context.Database.GetDbConnection();
    connection.Open();
    using var command = connection.CreateCommand();
    command.CommandText = "SELECT COUNT(*) FROM sys.foreign_keys WHERE parent_object_id=OBJECT_ID('Crates')";
    if (Convert.ToInt32(command.ExecuteScalar()) != 0) throw new InvalidOperationException("Constraint exists.");
    command.CommandText = "SELECT COUNT(*) FROM sys.indexes WHERE object_id=OBJECT_ID('Crates') AND name='IX_Crates_DepotId'";
    if (Convert.ToInt32(command.ExecuteScalar()) != 1) throw new InvalidOperationException("Database index missing.");
    command.CommandText = "SELECT COUNT(*) FROM __EFMigrationsHistory";
    if (Convert.ToInt32(command.ExecuteScalar()) != 1) throw new InvalidOperationException("Migration history missing.");
    command.CommandText = "INSERT INTO Crates (DepotId) VALUES (7301)";
    command.ExecuteNonQuery();
    Console.WriteLine("PASS applied history, absent FK constraint, retained model relationship and SQL index; database accepted missing principal.");
}
else Console.WriteLine("Quarry migration authoring consumer; no automatic database mutation.");

namespace QuarrySamples
{
    public sealed class QuarryContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlServer(
            Environment.GetEnvironmentVariable("PLAYBOOK_SQL_CONNECTION_STRING")
            ?? "Server=127.0.0.1,1;Database=unreachable;User ID=offline;Password=not-a-secret;Encrypt=False;Connect Timeout=1;ConnectRetryCount=0");
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Depot>().ToTable("Depots");
            var crate = modelBuilder.Entity<Crate>();
            crate.ToTable("Crates");
            crate.HasOne(c => c.Depot).WithMany().HasForeignKey(c => c.DepotId);
        }
    }
    public sealed class Depot { public int Id { get; set; } }
    public sealed class Crate { public int Id { get; set; } public int DepotId { get; set; } public Depot Depot { get; set; } = null!; }
    public sealed class Factory : IDesignTimeDbContextFactory<QuarryContext>
    {
        public QuarryContext CreateDbContext(string[] args) => new();
    }
}
// endsnippet:complete
