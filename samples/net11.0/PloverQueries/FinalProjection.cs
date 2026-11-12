// snippet:complete
using Microsoft.EntityFrameworkCore;
namespace PloverQueries;
public static class FinalProjection
{
    public static void Qualify(Rookery db)
    {
        var dto = db.Nests.LeftJoin(db.Counts.Select(c => new Reading { NestId = c.NestId, Birds = c.Birds }), n => n.Id, c => c.NestId, (n, c) => new { n.Id, Reading = c });
        var anon = db.Nests.LeftJoin(db.Counts.Select(c => new { c.NestId, c.Birds }), n => n.Id, c => c.NestId, (n, c) => new { n.Id, Reading = c });
        var value = db.Nests.LeftJoin(db.Counts.Select(c => new ReadingValue { NestId = c.NestId, Birds = c.Birds }), n => n.Id, c => c.NestId, (n, c) => new { n.Id, Reading = c });
        Probe("DTO", dto.Where(x => x.Id == 1).Select(x => x.Reading.Birds), dto.Select(x => x.Reading));
        Probe("anonymous", anon.Where(x => x.Id == 1).Select(x => x.Reading.Birds), anon.Select(x => x.Reading));
        Probe("default struct", value.Where(x => x.Id == 1).Select(x => x.Reading.Birds), value.Select(x => x.Reading));
        Probe("grouped DTO", dto.Where(x => x.Id == 1).Select(x => x.Reading.Birds), dto.GroupBy(x => x.Id).Select(g => g.Select(x => x.Reading).FirstOrDefault()));
        var nullable = db.Nests.LeftJoin(db.Counts.Select(c => (ReadingValue?)new ReadingValue { NestId = c.NestId, Birds = c.Birds }), n => n.Id, c => c!.Value.NestId, (n, c) => c);
        try { _ = nullable.ToQueryString(); }
        catch (InvalidOperationException ex) when (ex.Message.Contains("could not be translated"))
        {
            Console.WriteLine("PASS nullable struct: translation failure before execution");
            Fallback(db);
            return;
        }
        throw new Exception("Nullable struct translation changed; reassess route.");
    }
    private static void Probe<T>(string name, IQueryable<int> matched, IQueryable<T> all)
    {
        if (!matched.ToArray().SequenceEqual([12])) throw new Exception($"Wrong matched {name}");
        _ = all.ToQueryString();
        try { _ = all.ToArray(); }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Nullable object"))
        {
            Console.WriteLine($"PASS {name}: matched=12; unmatched materialization failure");
            return;
        }
        throw new Exception($"Direct {name} behavior changed; reassess route.");
    }
    private static void Fallback(Rookery db)
    {
        // Keep nullable scalars through SQL; construct whole objects after materialization.
        var query = db.Nests.LeftJoin(db.Counts.Select(c => new { Marker = (int?)c.Id, c.NestId, Birds = (int?)c.Birds }), n => n.Id, c => c.NestId, (n, c) => new { n.Id, c!.Marker, c.Birds });
        var rows = query.OrderBy(x => x.Id).ToArray();
        var dtos = rows.Select(x => x.Marker.HasValue ? new Reading { NestId = x.Id, Birds = x.Birds!.Value } : null).ToArray();
        var values = rows.Select(x => x.Marker.HasValue ? new ReadingValue { NestId = x.Id, Birds = x.Birds!.Value } : default).ToArray();
        var nullable = rows.Select(x => x.Marker.HasValue ? (ReadingValue?)new ReadingValue { NestId = x.Id, Birds = x.Birds!.Value } : null).ToArray();
        if (rows.Length != 2 || dtos[0]?.NestId != 1 || dtos[0]?.Birds != 12 || dtos[1] != null || values[0].NestId != 1 || values[0].Birds != 12 || values[1].NestId != 0 || values[1].Birds != 0 || nullable[0]?.NestId != 1 || nullable[0]?.Birds != 12 || nullable[1].HasValue) throw new Exception("Marker fallback lost null/default/populated results.");
        if (!query.Where(x => x.Marker != null && x.Birds > 10).Select(x => x.Id).ToArray().SequenceEqual([1]) || dtos.GroupBy(x => x == null).Count() != 2) throw new Exception("Marker composition failed.");
        Console.WriteLine("PASS marker fallback: DTO null, nullable null, struct default, populated (1,12); SQL filtering and client grouping");
    }
}
// endsnippet:complete
