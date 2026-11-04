// snippet:complete
#if DIAGNOSTIC_PROBE
using Microsoft.EntityFrameworkCore;
static class DiagnosticProbe
{
    // Compile only. Never execute these intentionally unsafe forms.
    public static async Task Inspect(DbContext db, string input)
    {
        _ = db.Set<Parcel>().FromSqlInterpolated($"SELECT * FROM Parcels WHERE Label = {input}");
        _ = db.Database.ExecuteSqlInterpolated($"DELETE FROM Parcels WHERE Label = {input}");
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM Parcels WHERE Label = {input}");
        _ = db.Set<Parcel>().FromSqlRaw(string.Format("SELECT * FROM Parcels WHERE Label = '{0}'", input));
        _ = db.Database.ExecuteSqlRaw(string.Concat("DELETE FROM Parcels WHERE Label = '", input, "'"));
        _ = db.Set<Parcel>().FromSqlRaw($"SELECT * FROM Parcels WHERE Label = '{input}'");
        _ = db.Database.SqlQueryRaw<int>($"SELECT Id FROM Parcels WHERE Label = '{input}'");
        _ = db.Database.ExecuteSqlRaw($"DELETE FROM Parcels WHERE Label = '{input}'");
    }
}
#endif

// endsnippet:complete
