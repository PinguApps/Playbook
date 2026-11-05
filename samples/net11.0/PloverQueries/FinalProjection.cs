// snippet:complete
using Microsoft.EntityFrameworkCore;

namespace PloverQueries;

public static class FinalProjection
{
    // Intended .NET 11 behavior from the official documentation.
    // This qualification MUST fail on the measured RC3 graph; it is not release proof.
    public static void Qualify(Rookery db)
    {
        var inner = db.Counts.Select(c => new Reading { NestId = c.NestId, Birds = c.Birds });
        var query = db.Nests.LeftJoin(inner, n => n.Id, c => c.NestId,
            (n, c) => new { n.Id, Reading = c });
        var rows = query.OrderBy(x => x.Id).ToArray();
        if (rows.Length != 2 || rows[0].Reading?.Birds != 12 || rows[1].Reading != null)
            throw new InvalidOperationException("Expected one DTO and one missing DTO.");

        var grouped = query.GroupBy(x => x.Id)
            .Select(g => g.Select(x => x.Reading).FirstOrDefault()).ToArray();
        if (grouped.Length != 2 || grouped.Count(x => x == null) != 1)
            throw new InvalidOperationException("Grouping lost missing-object semantics.");
        Console.WriteLine("PASS intended final DTO and grouped missing-object semantics");
    }
}
// endsnippet:complete
