// snippet:complete
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;

var connection = Environment.GetEnvironmentVariable("PLAYBOOK_SQL_CONNECTION_STRING") ?? throw new InvalidOperationException("Use isolated SQL Run.");
var services = new ServiceCollection().AddEntityFrameworkSqlServer().AddSingleton<IRelationalTypeMappingSourcePlugin, GaugeMappings>().BuildServiceProvider();
var options = new DbContextOptionsBuilder<GaugeContext>().UseSqlServer(connection).UseInternalServiceProvider(services).Options;
using (var context = new GaugeContext(options))
{
    context.Database.EnsureCreated();
    context.AddRange(new Gauge { Id=1, Reading=new Reading(7) },new Gauge { Id=2 });
    context.SaveChanges();
}
using (var context = new GaugeContext(options))
{
    var gauge=context.Set<Gauge>().Single(x=>x.Reading == new Reading(7));
    var property=context.Entry(gauge).Property(x=>x.Reading);
    gauge.Reading!.Value=9;
    context.ChangeTracker.DetectChanges();
    if (!property.IsModified) throw new InvalidOperationException("Mutable value was not detected.");
    context.SaveChanges();
    gauge.Reading=new Reading(9);
    context.ChangeTracker.DetectChanges();
    if (property.IsModified) throw new InvalidOperationException("Equal replacement was marked modified.");
}
using (var context = new GaugeContext(options))
{
    if(context.Set<Gauge>().Single(x=>x.Id==1).Reading?.Value!=9 || context.Set<Gauge>().Single(x=>x.Id==2).Reading is not null)
        throw new InvalidOperationException("Round-trip/null failed.");
    var map=context.Model.FindEntityType(typeof(Gauge))!.FindProperty(nameof(Gauge.Reading))!.GetRelationalTypeMapping();
    if(map.WithStoreTypeAndSize("int",null).ClrType != typeof(Reading)) throw new InvalidOperationException("Clone lost CLR type.");
}
Console.WriteLine("PASS scalar model/query/save/round-trip; mutation, equality, null and clone controls");
sealed class GaugeContext(DbContextOptions<GaugeContext> options):DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)=>modelBuilder.Entity<Gauge>().Property(x=>x.Id).ValueGeneratedNever();
}
sealed class Gauge { public int Id {get;set;} public Reading? Reading {get;set;} }
sealed class Reading(int value) { public int Value {get;set;}=value; }
sealed class GaugeMappings:IRelationalTypeMappingSourcePlugin
{
    public RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)=>mappingInfo.ClrType==typeof(Reading)?new ReadingMapping():null;
}
#if NET10_0
sealed class ReadingMapping:RelationalTypeMapping
#else
sealed class ReadingMapping:RelationalTypeMapping<Reading>
#endif
{
    public ReadingMapping():this(new RelationalTypeMappingParameters(new CoreTypeMappingParameters(typeof(Reading),
        converter:new ValueConverter<Reading,int>(x=>x.Value,x=>new Reading(x)),
        comparer:new ValueComparer<Reading>((a,b)=>a!.Value==b!.Value,x=>x.Value,x=>new Reading(x.Value))),"int",dbType:System.Data.DbType.Int32)) {}
    private ReadingMapping(RelationalTypeMappingParameters parameters):base(parameters) {}
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)=>new ReadingMapping(parameters);
    protected override string GenerateNonNullSqlLiteral(object value)=>((int)value).ToString(System.Globalization.CultureInfo.InvariantCulture);
}
// endsnippet:complete

