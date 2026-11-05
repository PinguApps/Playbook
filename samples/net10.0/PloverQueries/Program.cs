// snippet:complete
using Microsoft.EntityFrameworkCore;

namespace PloverQueries;
public static class Program { public static void Main() {
using var db = new Rookery();
db.Database.OpenConnection();
db.Database.EnsureCreated();
db.Nests.AddRange(new Nest { Id = 1, Coast = 7, Season = 2 }, new Nest { Id = 2, Coast = 7, Season = 2 });
db.Counts.Add(new Count { Id = 1, NestId = 1, Birds = 12 });
db.SaveChanges();
Console.WriteLine($"SQLite {db.Database.SqlQueryRaw<string>("select sqlite_version() as Value").Single()}");

void Check(string name, Func<int[]> query, int[] expected)
{
    var actual = query();
    if (!actual.SequenceEqual(expected)) throw new Exception($"{name}: {string.Join(",", actual)}");
    Console.WriteLine($"PASS {name}: {string.Join(",", actual)}");
}
void Probe(string name, Func<int[]> query)
{
#if NET11_0
    int[]? expected = name switch {
        "ValueTuple grouped key" or "Tuple grouped key" => [2],
        "ValueTuple joined key" => [12],
        "nested Rest" => [1,2], "one element" => [1], _ => null
    };
#else
    int[]? expected = null;
#endif
    int[] actual;
    try { actual = query(); }
    catch (InvalidOperationException ex) when (expected == null)
    {
        Console.WriteLine($"OBSERVED unsupported {name}: {ex.GetType().Name}: {ex.Message}");
        return;
    }
    if (expected == null) throw new Exception($"Previously failing {name} now succeeds; reassess availability: {string.Join(",",actual)}");
    if (!actual.SequenceEqual(expected)) throw new Exception($"Wrong results for {name}");
    Console.WriteLine($"PASS {name}: {string.Join(",",actual)}");
}
Check("anonymous grouped key", () => db.Nests.GroupBy(n => new { n.Coast, n.Season })
    .Where(g => g.Key.Coast == 7).Select(g => g.Count()).ToArray(), [2]);
Probe("ValueTuple grouped key", () => db.Nests.GroupBy(n => new ValueTuple<int,int>(n.Coast,n.Season))
    .Where(g => g.Key.Item1 == 7).Select(g => g.Count()).ToArray());
Probe("Tuple grouped key", () => db.Nests.GroupBy(n => new Tuple<int,int>(n.Coast,n.Season))
    .Where(g => g.Key.Item2 == 2).Select(g => g.Count()).ToArray());
Probe("ValueTuple joined key", () => db.Nests.Select(n => new ValueTuple<int,int>(n.Id,n.Coast))
    .Join(db.Counts, n => n.Item1, c => c.NestId, (n,c) => c.Birds).ToArray());
Probe("nested Rest", () => db.Nests.Select(n => new ValueTuple<int,int,int,int,int,int,int,ValueTuple<int>>(
    n.Id,n.Coast,n.Season,4,5,6,7,new ValueTuple<int>(n.Coast))).Where(t => t.Rest.Item1 == 7).Select(t => t.Item1).OrderBy(x=>x).ToArray());
Probe("one element", () => db.Nests.Select(n => new ValueTuple<int>(n.Id)).Where(t=>t.Item1==1).Select(t=>t.Item1).ToArray());
Probe("missing DTO", () => db.Nests.LeftJoin(db.Counts.Select(c=>new Reading { NestId=c.NestId,Birds=c.Birds }), n=>n.Id,c=>c.NestId,
    (n,c)=>c).ToArray().Select(c=>c == null ? -1 : c.Birds).OrderBy(x=>x).ToArray());
Probe("missing anonymous", () => db.Nests.LeftJoin(db.Counts.Select(c=>new {c.NestId,c.Birds}),n=>n.Id,c=>c.NestId,
    (n,c)=>c).ToArray().Select(c=>c == null ? -1 : c.Birds).OrderBy(x=>x).ToArray());
Probe("missing nullable struct", () => db.Nests.LeftJoin(db.Counts.Select(c=>(ReadingValue?)new ReadingValue { NestId=c.NestId,Birds=c.Birds }),
    n=>n.Id,c=>c!.Value.NestId,(n,c)=>c).ToArray().Select(c=>c.HasValue ? c.Value.Birds : -1).OrderBy(x=>x).ToArray());
Check("explicit marker", () => db.Nests.LeftJoin(db.Counts.Select(c=>new { Marker=(int?)c.Id,c.NestId,Birds=(int?)c.Birds }),
    n=>n.Id,c=>c.NestId,(n,c)=> c!.Marker == null ? -1 : c.Birds!.Value).OrderBy(x=>x).ToArray(),[-1,12]);

}}
public class Reading { public int NestId {get;set;} public int Birds {get;set;} }
public struct ReadingValue { public int NestId {get;set;} public int Birds {get;set;} }
public class Nest { public int Id {get;set;} public int Coast {get;set;} public int Season {get;set;} }
public class Count { public int Id {get;set;} public int NestId {get;set;} public int Birds {get;set;} }
public class Rookery : DbContext
{
 public DbSet<Nest> Nests => Set<Nest>();
 public DbSet<Count> Counts => Set<Count>();
 protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite("Data Source=:memory:").LogTo(Console.WriteLine, [Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.CommandExecuted]);
}
// endsnippet:complete




