// snippet:complete
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

using var connection = new SqliteConnection("Data Source=:memory:");
connection.Open();
var options = new DbContextOptionsBuilder<Aviary>().UseSqlite(connection).Options;
using (var db = new Aviary(options))
{
    db.Database.EnsureCreated();
    db.Database.ExecuteSql($"INSERT INTO Birds (Id, Name) VALUES (1, {"Skylark"})");
}
using (var db = new Aviary(options))
{
    var bird = db.Set<Bird>().Single();
    if (bird.Name != "Skylark" || !ReferenceEquals(bird.Owner, db)) throw new Exception("Constructor/service binding failed.");
}
using (var db = new Aviary(options))
{
    var factory = db.GetService<IConstructorBindingFactory>();
    var model = new ModelBuilder();
    var valid = model.Entity<Bird>().Metadata;
    model.Entity<Bird>().Property(x => x.Id);
    model.Entity<Bird>().Property(x => x.Name);
#if NET10_0
#pragma warning disable CS0618
    if (!factory.TryBindConstructor(valid, typeof(Bird).GetConstructors().Single(), out var binding, out _))
        throw new Exception("Expected a valid explicit constructor.");
    var invalid = model.Entity<InvalidBird>().Metadata;
    if (factory.TryBindConstructor(invalid, typeof(InvalidBird).GetConstructors().Single(), out _, out var missing)
        || !missing.Any(x => x.Name == "unknown")) throw new Exception("Expected unbound parameter.");
#pragma warning restore CS0618
#else
    factory.GetBindings(valid, out var binding, out _);
    var invalid = model.Entity<InvalidBird>().Metadata;
    try { factory.GetBindings(invalid, out _, out _); throw new Exception("Expected invalid binding rejection."); }
    catch (InvalidOperationException error) when (error.Message.Contains("unknown", StringComparison.Ordinal)) { }
#endif
    if (binding.ParameterBindings.Count != 3) throw new Exception("Expected scalar/scalar/service parameters.");
}
Console.WriteLine("PASS constructor scalar/service materialization, binding and invalid parameter control");
Console.WriteLine("SQLite " + connection.ServerVersion);
sealed class Aviary(DbContextOptions<Aviary> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<Bird>().ToTable("Birds");
}
sealed class Bird(int id, string name, DbContext context)
{
    public int Id { get; private set; } = id;
    public string Name { get; private set; } = name;
    public DbContext Owner { get; } = context;
}
sealed class InvalidBird(Uri unknown) { public Uri Unknown { get; } = unknown; }
// endsnippet:complete
