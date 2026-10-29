// snippet:complete
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.Data.Sqlite;

if (!OperatingSystem.IsWindows() || RuntimeInformation.ProcessArchitecture != Architecture.X64)
    throw new PlatformNotSupportedException("This verified route requires Windows x64 and its system winsqlite3.dll.");

// Do this once, before constructing any SqliteConnection or starting database workers.
// The provider selects the existing Windows library; it does not ship an encrypted engine.
if (args.Length == 0 || args[0] != "--omit-initialization")
    SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_winsqlite3());

var database = Path.Combine(Path.GetTempPath(), "canal-permits-" + Guid.NewGuid().ToString("N") + ".db");
var settings = new SqliteConnectionStringBuilder { DataSource = database, Pooling = false };
try
{
    using (var connection = new SqliteConnection(settings.ToString()))
    {
        connection.Open();
        using var create = connection.CreateCommand();
        create.CommandText = "CREATE TABLE permit (code TEXT PRIMARY KEY, remaining INTEGER NOT NULL CHECK (remaining >= 0));";
        create.ExecuteNonQuery();
        using var insert = connection.CreateCommand();
        insert.CommandText = "INSERT INTO permit VALUES ($code, 3);";
        insert.Parameters.AddWithValue("$code", "King's Lock");
        insert.ExecuteNonQuery();
        using var transaction = connection.BeginTransaction();
        using var debit = connection.CreateCommand();
        debit.Transaction = transaction;
        debit.CommandText = "UPDATE permit SET remaining = remaining - 1 WHERE code = $code;";
        debit.Parameters.AddWithValue("$code", "King's Lock");
        if (debit.ExecuteNonQuery() != 1) throw new InvalidOperationException("Permit not found.");
        transaction.Commit();
    }

    using (var reopened = new SqliteConnection(settings.ToString()))
    {
        reopened.Open();
        using var read = reopened.CreateCommand();
        read.CommandText = "SELECT remaining FROM permit WHERE code = $code;";
        read.Parameters.AddWithValue("$code", "King's Lock");
        if (Convert.ToInt64(read.ExecuteScalar()) != 2) throw new InvalidOperationException("Committed permit debit was lost.");
        using var version = reopened.CreateCommand();
        version.CommandText = "SELECT sqlite_version();";
        Console.WriteLine($"Native SQLite {version.ExecuteScalar()}; managed {typeof(SqliteConnection).Assembly.GetName().Version}; runtime {Environment.Version}");
    }

    // A password is not encryption support. Refuse this engine for encrypted storage.
    var encrypted = new SqliteConnectionStringBuilder(settings.ToString()) { Password = "probe-only-not-a-secret" };
    try
    {
        using var rejected = new SqliteConnection(encrypted.ToString());
        rejected.Open();
        throw new InvalidOperationException("Unexpected password acceptance by the unencrypted engine.");
    }
    catch (InvalidOperationException exception) when (exception.Message.Contains("doesn't support encryption", StringComparison.Ordinal))
    {
        Console.WriteLine("PASS: system engine rejects a password; encryption requires another verified native route.");
    }

    var native = Process.GetCurrentProcess().Modules.Cast<ProcessModule>()
        .Single(module => string.Equals(module.ModuleName, "winsqlite3.dll", StringComparison.OrdinalIgnoreCase));
    Console.WriteLine($"Loaded {native.FileName}; file {native.FileVersionInfo.FileVersion}");
    Console.WriteLine($"Native SHA256 {Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(native.FileName)))}");
    Console.WriteLine("PASS: native initialization, parameterized permit debit, transaction commit and file reopen.");
}
finally
{
    File.Delete(database);
}
// endsnippet:complete
