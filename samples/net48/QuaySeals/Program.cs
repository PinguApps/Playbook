// snippet:complete
using System;
using System.IO;
using System.Reflection;
using Microsoft.Data.Sqlite;

internal static class Program
{
    private static void Main()
    {
        string database = Path.Combine(Path.GetTempPath(), "quay-seals-" + Guid.NewGuid().ToString("N") + ".db");
        var settings = new SqliteConnectionStringBuilder { DataSource = database, Pooling = false };
        const string seal = "West quay's locker 71";
        try
        {
            using (var connection = new SqliteConnection(settings.ToString()))
            {
                connection.Open();
                using (var create = connection.CreateCommand())
                {
                    create.CommandText = "CREATE TABLE SealLog (id INTEGER PRIMARY KEY, seal_code TEXT NOT NULL)";
                    create.ExecuteNonQuery();
                }
                using (var transaction = connection.BeginTransaction())
                using (var insert = connection.CreateCommand())
                {
                    insert.Transaction = transaction;
                    insert.CommandText = "INSERT INTO SealLog VALUES ($id, $seal)";
                    insert.Parameters.AddWithValue("$id", 71);
                    insert.Parameters.AddWithValue("$seal", seal);
                    if (insert.ExecuteNonQuery() != 1) throw new Exception("Insert did not affect one row.");
                    transaction.Commit();
                }
                using (var transaction = connection.BeginTransaction())
                using (var update = connection.CreateCommand())
                {
                    update.Transaction = transaction;
                    update.CommandText = "UPDATE SealLog SET seal_code = $seal WHERE id = $id";
                    update.Parameters.AddWithValue("$id", 71);
                    update.Parameters.AddWithValue("$seal", "unconfirmed replacement");
                    if (update.ExecuteNonQuery() != 1) throw new Exception("Rollback control did not update a row.");
                    transaction.Rollback();
                }
            }
            // A new connection must see the committed original after the rollback.
            using (var reopened = new SqliteConnection(settings.ToString()))
            {
                reopened.Open();
                using (var read = reopened.CreateCommand())
                {
                    read.CommandText = "SELECT seal_code FROM SealLog WHERE id = $id";
                    read.Parameters.AddWithValue("$id", 71);
                    if (!seal.Equals(read.ExecuteScalar()))
                        throw new Exception("The committed seal did not survive reopening.");
                }
                var provider = typeof(SqliteConnection).Assembly
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
                Console.WriteLine("PASS: parameterized seal, commit, rollback and file reopen; CLR " +
                    Environment.Version + "; provider " + provider + "; SQLite " + reopened.ServerVersion);
            }
        }
        finally
        {
            if (File.Exists(database)) File.Delete(database);
        }
    }
}
// endsnippet:complete
