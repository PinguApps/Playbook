// snippet:complete
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FenWatch;

internal static class Program
{
    public static async Task Main()
    {
        await using SqliteConnection connection = new("Data Source=:memory:");
        await connection.OpenAsync();
        await using (SqliteCommand setup = connection.CreateCommand())
        {
            setup.CommandText = """
                CREATE TABLE Surveys (Id INTEGER PRIMARY KEY, Station TEXT NOT NULL, Ready INTEGER NOT NULL);
                INSERT INTO Surveys VALUES (1, 'Reed', 1), (2, 'Sedge', 0), (3, 'Mallow', 1);
                """;
            await setup.ExecuteNonQueryAsync();
        }

        QueryObserver observer = new();
        DbContextOptions<SurveyContext> options = new DbContextOptionsBuilder<SurveyContext>()
            .UseSqlite(connection).AddInterceptors(observer).Options;
        await using SurveyContext context = new(options);
        IQueryable<string> query = context.Surveys.AsNoTracking()
            .Where(survey => survey.Ready).OrderBy(survey => survey.Id)
            .Select(survey => survey.Station);

        // Keep the server-side filter/projection before the adapter.
        IAsyncEnumerable<string> stations = query.ToAsyncEnumerable();
        Require(observer.Commands == 0, "constructing the stream must not query");
        await CheckEnumeration(stations, observer, asynchronous: false);

        // AsAsyncEnumerable already works in EF10; this is the valid older fix.
        IAsyncEnumerable<string> asynchronousReplacement = query.AsAsyncEnumerable();
        await CheckEnumeration(asynchronousReplacement, observer, asynchronous: true);

        observer.Reset();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        bool cancelled = false;
        try
        {
            await foreach (string station in asynchronousReplacement.WithCancellation(cancellation.Token))
            {
                throw new InvalidOperationException($"Unexpected cancelled result: {station}");
            }
        }
        catch (OperationCanceledException)
        {
            cancelled = true;
        }
        Require(cancelled && observer.Disposals == observer.Readers, "pre-cancellation leaves no reader");
        Console.WriteLine($"PASS: query adapter, deferred/repeated execution, break/exception disposal and cancellation; runtime {Environment.Version}; SQLite {connection.ServerVersion}");
    }

    private static async Task CheckEnumeration(IAsyncEnumerable<string> stations, QueryObserver observer, bool asynchronous)
    {
        observer.Reset();
        await foreach (string station in stations)
        {
            Require(station == "Reed", "first selected station");
            break;
        }
        Require(observer.Commands == 1 && observer.Disposals == 1, "break disposes the actual reader");
        Require(observer.AsyncCommands == (asynchronous ? 1 : 0), "actual command execution path");

        observer.Reset();
        for (int pass = 0; pass < 2; pass++)
        {
            List<string> received = [];
            await foreach (string station in stations) { received.Add(station); }
            Require(received.SequenceEqual(["Reed", "Mallow"]), "server filter, order and projection");
        }
        Require(observer.Commands == 2 && observer.Disposals == 2, "enumerating twice queries twice");
        Require(observer.AsyncCommands == (asynchronous ? 2 : 0), "adapter is not a result cache");

        observer.Reset();
        try
        {
            await foreach (string station in stations) { throw new IOException(station); }
        }
        catch (IOException) { }
        Require(observer.Commands == 1 && observer.Disposals == 1, "consumer exception disposes the reader");
        Console.WriteLine($"Observed {(asynchronous ? "async" : "sync")} EF commands: break=1, repeated=2, exception=1; all readers disposed.");
    }

    private static void Require(bool condition, string claim)
    {
        if (!condition) { throw new InvalidOperationException(claim); }
    }
}

internal sealed class SurveyContext(DbContextOptions<SurveyContext> options) : DbContext(options)
{
    public DbSet<Survey> Surveys => Set<Survey>();
}

internal sealed class Survey
{
    public int Id { get; set; }
    public required string Station { get; set; }
    public bool Ready { get; set; }
}

internal sealed class QueryObserver : DbCommandInterceptor
{
    public int Commands { get; private set; }
    public int AsyncCommands { get; private set; }
    public int Disposals { get; private set; }
    public int Readers { get; private set; }

    public void Reset() { Commands = 0; AsyncCommands = 0; Disposals = 0; Readers = 0; }

    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        Commands++;
        return result;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    {
        Commands++;
        AsyncCommands++;
        return ValueTask.FromResult(result);
    }

    public override InterceptionResult DataReaderDisposing(DbCommand command, DataReaderDisposingEventData eventData, InterceptionResult result)
    {
        Disposals++;
        return result;
    }

    public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
    {
        Readers++;
        return result;
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
    {
        Readers++;
        return ValueTask.FromResult(result);
    }
}
// endsnippet:complete
