// snippet:complete
using System.Globalization;
using Microsoft.Data.Sqlite;

namespace MarshCounters;

internal static class Program
{
    public static void Main()
    {
        using SqliteConnection connection = new("Data Source=:memory:");
        connection.Open();
        using (SqliteCommand setup = connection.CreateCommand())
        {
            // TEXT affinity retains the padding; NUMERIC/INTEGER/REAL can lose it.
            setup.CommandText = "CREATE TABLE Counters (Value TEXT COLLATE BINARY NOT NULL)";
            setup.ExecuteNonQuery();
        }

        UInt128[] values = [UInt128.MaxValue, 10, (UInt128)ulong.MaxValue + 1, 0, 100, 9, (UInt128)long.MaxValue + 1, 99, UInt128.MaxValue - 1];
        foreach (UInt128 value in values)
        {
            using SqliteCommand insert = connection.CreateCommand();
            insert.CommandText = "INSERT INTO Counters (Value) VALUES ($value)";
            // Provider 11 binds the UInt128 as invariant, zero-padded D39 TEXT.
            insert.Parameters.AddWithValue("$value", value);
            Require(insert.ExecuteNonQuery() == 1, "one inserted counter");
        }

        UInt128[] expected = values.Order().ToArray();
        using (SqliteCommand ordered = connection.CreateCommand())
        {
            ordered.CommandText = "SELECT Value, typeof(Value), length(Value) FROM Counters ORDER BY Value COLLATE BINARY";
            using SqliteDataReader reader = ordered.ExecuteReader();
            int index = 0;
            while (reader.Read())
            {
                string stored = reader.GetString(0);
                Require(index < expected.Length && stored == expected[index].ToString("D39", CultureInfo.InvariantCulture), "exact full-width ordered storage");
                Require(reader.GetString(1) == "text" && reader.GetInt32(2) == 39, "39-digit TEXT, including zero");
                // Application parsing is explicit; the provider has no UInt128 reader API.
                Require(UInt128.Parse(stored, CultureInfo.InvariantCulture) == expected[index], "explicit string parse retains all bits");
                Console.WriteLine(stored);
                index++;
            }
            Require(index == expected.Length, "all counters inspected");
        }

        using (SqliteCommand range = connection.CreateCommand())
        {
            range.CommandText = "SELECT count(*) FROM Counters WHERE Value >= $minimum";
            range.Parameters.AddWithValue("$minimum", (UInt128)ulong.MaxValue + 1);
            Require((long)range.ExecuteScalar()! == 3, "parameter comparison shares the ordered representation");
        }
        Console.WriteLine($"PASS: UInt128 D39 TEXT storage, full range, ordering and parameter comparison; runtime {Environment.Version}; SQLite {connection.ServerVersion}");
    }

    private static void Require(bool condition, string claim)
    {
        if (!condition) { throw new InvalidOperationException(claim); }
    }
}
// endsnippet:complete
