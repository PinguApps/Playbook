// snippet:complete
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;

using var connection = new SqliteConnection("Data Source=:memory:");
connection.Open();
var options = new DbContextOptionsBuilder<PermitContext>().UseSqlite(connection)
    .ReplaceService<IValueGeneratorSelector, PermitSelector>().Options;
using (var context = new PermitContext(options))
{
    context.Database.EnsureCreated();
    context.AddRange(new Permit(), new Permit(), new Permit());
    context.SaveChanges();
    var keys = context.Set<Permit>().Select(x => x.Id).ToArray();
    if (keys.Length != 3 || keys.Distinct().Count() != 3 || keys.Any(x => !x.StartsWith("tern-", StringComparison.Ordinal)))
        throw new InvalidOperationException("Generated keys were not persisted.");
    var type = context.Model.FindEntityType(typeof(Permit))!;
    var unsupported = type.FindProperty(nameof(Permit.Unsupported))!;
    var selector = context.GetService<IValueGeneratorSelector>();
#if NET10_0
#pragma warning disable CS0618
    try { selector.Select(unsupported, type); throw new InvalidOperationException("Expected unsupported generator failure."); }
    catch (NotSupportedException) { }
#pragma warning restore CS0618
#else
    if (selector.TrySelect(unsupported, type, out var missing) || missing is not null)
        throw new InvalidOperationException("Unsupported type unexpectedly received a generator.");
#endif
}
using (var fresh = new PermitContext(options))
    if (fresh.Set<Permit>().Count() != 3) throw new InvalidOperationException("Fresh read lost permits.");
Console.WriteLine("PASS generator selection, three unique persisted keys, fresh read and no-generator control");
sealed class PermitContext(DbContextOptions<PermitContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Permit>().Property(x => x.Id).ValueGeneratedOnAdd();
    }
}
sealed class Permit { public string Id { get; set; } = null!; public decimal Unsupported { get; set; } }
sealed class PermitGenerator : ValueGenerator<string>
{
    public override bool GeneratesTemporaryValues => false;
    public override string Next(EntityEntry entry) => "tern-" + Guid.NewGuid().ToString("N");
}
sealed class PermitSelector(ValueGeneratorSelectorDependencies dependencies) : ValueGeneratorSelector(dependencies)
{
#if NET10_0
#pragma warning disable CS0618
    [Obsolete("Legacy EF10 comparison; migrate to TryCreate.")]
    public override ValueGenerator Create(IProperty property, ITypeBase typeBase)
        => property.Name == nameof(Permit.Id) && typeBase.ClrType == typeof(Permit)
            ? new PermitGenerator() : base.Create(property, typeBase);
#pragma warning restore CS0618
#endif
    public override bool TryCreate(IProperty property, ITypeBase typeBase, [NotNullWhen(true)] out ValueGenerator? valueGenerator)
    {
        if (property.Name == nameof(Permit.Id) && typeBase.ClrType == typeof(Permit))
        { valueGenerator = new PermitGenerator(); return true; }
        return base.TryCreate(property, typeBase, out valueGenerator);
    }
}
// endsnippet:complete


