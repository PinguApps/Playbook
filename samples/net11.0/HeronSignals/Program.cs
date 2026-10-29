// snippet:complete
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HeronSignals;

internal static class Program
{
    public static void Main()
    {
        Console.WriteLine($"Runtime {RuntimeInformation.FrameworkDescription}; {RuntimeInformation.OSDescription}; {RuntimeInformation.ProcessArchitecture}; EF {typeof(DbContext).Assembly.GetName().Version}; Sqlite {typeof(SqliteConnection).Assembly.GetName().Version}");
        using SqliteConnection connection = new("Data Source=:memory:");
        connection.Open();
        using (SqliteCommand version = connection.CreateCommand())
        {
            version.CommandText = "SELECT sqlite_version(), sqlite_source_id()";
            using SqliteDataReader reader = version.ExecuteReader();
            Require(reader.Read(), "native version row");
            Console.WriteLine($"SQLite {reader.GetString(0)}; source {reader.GetString(1)}");
        }
        using (Process process = Process.GetCurrentProcess())
        {
            foreach (ProcessModule module in process.Modules)
            {
                if (Path.GetFileNameWithoutExtension(module.FileName).Equals("e_sqlite3", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Native {module.FileName}; SHA256 {Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(module.FileName)))}");
                }
            }
        }
        CommandProbe probe = new();
        DbContextOptions<SignalContext> options = new DbContextOptionsBuilder<SignalContext>()
            .UseSqlite(connection).AddInterceptors(probe).Options;
        TimeOnly?[] times =
        [
            TimeOnly.MinValue,
            TimeOnly.ParseExact("07:14:59.9994000", "HH:mm:ss.fffffff", CultureInfo.InvariantCulture),
            TimeOnly.ParseExact("07:14:59.9995000", "HH:mm:ss.fffffff", CultureInfo.InvariantCulture),
            TimeOnly.ParseExact("07:14:59.9999999", "HH:mm:ss.fffffff", CultureInfo.InvariantCulture),
            new(7, 15, 0),
            TimeOnly.ParseExact("07:15:42.1234567", "HH:mm:ss.fffffff", CultureInfo.InvariantCulture),
            TimeOnly.ParseExact("07:15:42.9999999", "HH:mm:ss.fffffff", CultureInfo.InvariantCulture),
            TimeOnly.MaxValue,
            null,
            TimeOnly.ParseExact("23:59:59.9994000", "HH:mm:ss.fffffff", CultureInfo.InvariantCulture),
            new(8, 15, 42)
        ];
        using (SignalContext setup = new(options))
        {
            setup.Database.EnsureCreated();
            for (int index = 0; index < times.Length; index++)
            {
                setup.Signals.Add(new Signal { Id = index + 1, Station = "reed-hide", RingsAt = times[index] });
            }
            setup.SaveChanges();
        }
        // Inspect actual storage and SQLite components before executing the application filters.
        using (SqliteCommand stored = connection.CreateCommand())
        {
            stored.CommandText = "SELECT Id, typeof(RingsAt), RingsAt, strftime('%H', RingsAt), strftime('%M', RingsAt), strftime('%S', RingsAt) FROM Signals ORDER BY Id";
            Console.WriteLine(stored.CommandText);
            using SqliteDataReader reader = stored.ExecuteReader();
            int count = 0;
            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                TimeOnly? expected = times[id - 1];
                Require(reader.GetString(1) == (expected.HasValue ? "text" : "null"), "default TEXT/null storage");
                string text = reader.IsDBNull(2) ? "NULL" : reader.GetString(2);
                Require(text == (expected.HasValue ? expected.Value.ToString(expected.Value.Ticks % TimeSpan.TicksPerSecond == 0 ? "HH:mm:ss" : "HH:mm:ss.fffffff", CultureInfo.InvariantCulture) : "NULL"), "actual stored precision");
                string parts = string.Join(':', Enumerable.Range(3, 3).Select(i => reader.IsDBNull(i) ? "NULL" : reader.GetString(i)));
                Require(parts == (expected?.ToString("HH:mm:ss", CultureInfo.InvariantCulture) ?? "NULL:NULL:NULL"), "native whole components at precision boundaries");
                Console.WriteLine($"Stored {id}: {text}; SQLite H:M:S {parts}; CLR H:M:S {(expected.HasValue ? $"{expected.Value.Hour}:{expected.Value.Minute}:{expected.Value.Second}" : "NULL")}");
                count++;
            }
            Require(count == times.Length, "all stored boundary inputs");
        }
        using SignalContext context = new(options);
        Signal[] roundTrip = context.Signals.AsNoTracking().OrderBy(s => s.Id).ToArray();
        Require(roundTrip.Select(s => s.RingsAt).SequenceEqual(times), "fresh-context TimeOnly round-trip retains ticks");
        // Only this bounded verification reads the fixture back; application filtering stays IQueryable.
        probe.Commands.Clear();
        RejectUnsupported(context, probe);
        Run(context, probe, "hour", 7, null, null, "reed-hide");
        Run(context, probe, "minute", null, 15, null, "reed-hide");
        Run(context, probe, "second", null, null, 42, "reed-hide");
        Run(context, probe, "combined", 7, 15, 42, "reed-hide");
        Run(context, probe, "midnight", 0, 0, 0, "reed-hide");
        Run(context, probe, "late", 23, 59, 59, "reed-hide");
        Run(context, probe, "parameter-control", 7, 15, 42, "reed-hide' OR 1=1 --");
        Console.WriteLine("PASS: actual stored precision, native components, fresh-context tick round-trip and executed server filters.");
    }

    private static void Run(SignalContext context, CommandProbe probe, string label,
        int? hour, int? minute, int? second, string station)
    {
        IQueryable<Signal> query = context.Signals.AsNoTracking().Where(s => s.Station == station && s.RingsAt != null);
        if (hour.HasValue) { int value = hour.Value; query = query.Where(s => s.RingsAt!.Value.Hour == value); }
        if (minute.HasValue) { int value = minute.Value; query = query.Where(s => s.RingsAt!.Value.Minute == value); }
        if (second.HasValue) { int value = second.Value; query = query.Where(s => s.RingsAt!.Value.Second == value); }
        int[] expected = label switch
        {
            "hour" => [2, 3, 4, 5, 6, 7], "minute" => [5, 6, 7, 11], "second" => [6, 7, 11],
            "combined" => [6, 7], "midnight" => [1], "late" => [8, 10], "parameter-control" => [],
            _ => throw new ArgumentException("Unknown check", nameof(label))
        };
        int before = probe.Commands.Count;
        int[] ids = query.OrderBy(s => s.Id).Select(s => s.Id).ToArray();
        Require(probe.Commands.Count == before + 1, "one database command per filter");
        Require(ids.SequenceEqual(expected), $"{label}: exact result IDs");
        string sql = probe.Commands[^1];
        foreach (string format in new[] { hour.HasValue ? "%H" : "", minute.HasValue ? "%M" : "", second.HasValue ? "%S" : "" }.Where(f => f.Length != 0))
        {
            Require(sql.Contains($"strftime('{format}'", StringComparison.Ordinal) && sql.Contains("WHERE", StringComparison.Ordinal), "executed server component predicate");
        }
        Require(!sql.Contains(station, StringComparison.Ordinal) && probe.LastParameters.ContainsValue(station), "station remains a parameter");
        Console.WriteLine($"Result {label}: [{string.Join(',', ids)}]");
    }

    private static void RejectUnsupported(SignalContext context, CommandProbe probe)
    {
        int before = probe.Commands.Count;
        try
        {
            _ = context.Signals.Where(s => s.RingsAt!.Value.Millisecond == 123).Select(s => s.Id).ToArray();
            throw new InvalidOperationException("Unexpected millisecond translation");
        }
        catch (InvalidOperationException exception) when (exception.Message.Contains("could not be translated", StringComparison.Ordinal))
        {
            Require(probe.Commands.Count == before, "unsupported member fails before any database read");
            Console.WriteLine("PASS: Millisecond predicate rejected before execution; no client-evaluation fallback.");
        }
    }

    internal static void Require(bool condition, string claim)
    {
        if (!condition) { throw new InvalidOperationException(claim); }
    }
}

internal sealed class SignalContext(DbContextOptions<SignalContext> options) : DbContext(options)
{
    public DbSet<Signal> Signals => Set<Signal>();
}

internal sealed class Signal
{
    public int Id { get; set; }
    public string Station { get; set; } = "";
    public TimeOnly? RingsAt { get; set; }
}

internal sealed class CommandProbe : DbCommandInterceptor
{
    public List<string> Commands { get; } = [];
    public Dictionary<string, object?> LastParameters { get; } = [];

    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command,
        CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        Commands.Add(command.CommandText);
        LastParameters.Clear();
        Console.WriteLine($"Executed SQL: {command.CommandText}");
        foreach (DbParameter parameter in command.Parameters)
        {
            LastParameters.Add(parameter.ParameterName, parameter.Value);
            Console.WriteLine($"Parameter {parameter.ParameterName} ({parameter.DbType}): {parameter.Value}");
        }
        return result;
    }
}
// endsnippet:complete
