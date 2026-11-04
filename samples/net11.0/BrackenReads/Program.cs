// snippet:complete
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BrackenReads;

internal static class Program
{
    internal static readonly string Connection = Environment.GetEnvironmentVariable("PLAYBOOK_SQL_CONNECTION_STRING") ?? throw new InvalidOperationException("Use the isolated SQL route.");
    internal static DbContextOptions<ReadContext> Options(DbCommandInterceptor? interceptor = null)
    {
        DbContextOptionsBuilder<ReadContext> builder = new DbContextOptionsBuilder<ReadContext>().UseSqlServer(Connection);
        if (interceptor is not null) { builder.AddInterceptors(interceptor); }
        return builder.Options;
    }
    private static async Task Main()
    {
        await using (ReadContext setup = new(Options()))
        {
            await setup.Database.EnsureCreatedAsync();
            setup.AddRange(new Tray { Id = 10, Cards = [new Card { Id = 10 }] }, new Tray { Id = 30, Cards = [new Card { Id = 30 }] });
            await setup.SaveChangesAsync();
        }
        InsertBetweenCommands race = new();
        try { await using ReadContext reader = new(Options(race)); await reader.Trays.Include(x => x.Cards).AsSplitQuery().AsNoTracking().OrderBy(x => x.Id).ToListAsync(); throw new InvalidOperationException("Expected query concurrency failure."); } catch (DbQueryConcurrencyException) { Console.WriteLine("Observed actual DbQueryConcurrencyException; discarding the failed context and retrying the entire bounded read once."); }
        await using ReadContext retry = new(Options());
        List<Tray> recovered = await retry.Trays.Include(x => x.Cards).AsSplitQuery().AsNoTracking().OrderBy(x => x.Id).ToListAsync();
        if (string.Join(",", recovered.Select(x => x.Id)) != "10,20,30" || recovered.Any(x => x.Cards.Count != 1)) { throw new InvalidOperationException("Retry did not reload the complete graph."); }
        Console.WriteLine("PASS: separate SQL writer inserted tray/card 20 between root and child commands; fresh-context whole-read retry returned 10/20/30 with one card each. This does not establish a transaction snapshot or arbitrary provider consistency.");
    }
}
internal sealed class InsertBetweenCommands : DbCommandInterceptor
{
    private int commands;
    public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    {
        if (++commands == 2)
        {
            await using ReadContext writer = new(Program.Options());
            writer.Add(new Tray { Id = 20, Cards = [new Card { Id = 20 }] });
            await writer.SaveChangesAsync(cancellationToken);
            Console.WriteLine("Committed concurrent writer before child SELECT.");
        }
        return result;
    }
}
internal sealed class ReadContext(DbContextOptions<ReadContext> options) : DbContext(options)
{
    public DbSet<Tray> Trays => Set<Tray>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tray>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<Card>().Property(x => x.Id).ValueGeneratedNever();
    }
}
internal sealed class Tray
{
    public int Id { get; set; }
    public List<Card> Cards { get; set; } = [];
}
internal sealed class Card
{
    public int Id { get; set; }
    public int TrayId { get; set; }
}
// endsnippet:complete
