// snippet:complete
using Microsoft.EntityFrameworkCore;
namespace CurlewFileModel;
public sealed class SurveyContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite("Data Source=curlew-model.db");
    public DbSet<Nest> Nests => Set<Nest>();
}
public sealed class Nest { public int Id { get; set; } public string Code { get; set; } = ""; }
// endsnippet:complete
