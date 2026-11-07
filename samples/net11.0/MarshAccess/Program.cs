// snippet:complete
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.SqlServer;

// A deployed host supplies its own server/database and identity permissions.
var connectionString = Environment.GetEnvironmentVariable("MARSH_SQL_CONNECTION")
    ?? "Server=tcp:marsh.invalid,1433;Database=MarshObservations;Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False";
var connection = new SqlConnectionStringBuilder(connectionString);
if (connection.Authentication != SqlAuthenticationMethod.ActiveDirectoryDefault)
    throw new InvalidOperationException("This sample requires Active Directory Default.");

var services = new ServiceCollection();
services.AddDbContext<MarshContext>(options => options.UseSqlServer(connectionString));
services.AddDistributedSqlServerCache(options =>
{
    options.ConnectionString = connectionString;
    options.SchemaName = "dbo";
    options.TableName = "ObservationCache";
});
using var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();
var database = scope.ServiceProvider.GetRequiredService<MarshContext>();
var cacheOptions = provider.GetRequiredService<IOptions<SqlServerCacheOptions>>().Value;
var efConnection = new SqlConnectionStringBuilder(database.Database.GetConnectionString());
var cacheConnection = new SqlConnectionStringBuilder(cacheOptions.ConnectionString);
if (efConnection.Authentication != cacheConnection.Authentication || efConnection.DataSource != cacheConnection.DataSource || efConnection.InitialCatalog != cacheConnection.InitialCatalog)
    throw new InvalidOperationException("EF and cache must use the same intended identity route.");
if (SqlAuthenticationProvider.GetProvider(SqlAuthenticationMethod.ActiveDirectoryDefault) is null)
    throw new InvalidOperationException("Built-in Entra provider is missing. Review direct Azure extension references.");
_ = provider.GetRequiredService<IDistributedCache>();
Console.WriteLine("PASS EF and distributed cache configuration; built-in Entra provider available. No token or SQL login attempted.");

// Run only on an authorized target with SELECT and cache-table DML grants.
if (args.Contains("--connect", StringComparer.Ordinal))
{
    await database.Database.OpenConnectionAsync();
    await database.Database.CloseConnectionAsync();
    var cache = provider.GetRequiredService<IDistributedCache>();
    var key = "marsh-proof-" + Guid.NewGuid().ToString("N");
    try
    {
        await cache.SetStringAsync(key, "observation-ready");
        if (await cache.GetStringAsync(key) != "observation-ready")
            throw new InvalidOperationException("Cache round trip failed.");
    }
    finally { await cache.RemoveAsync(key); }
    Console.WriteLine("PASS actual EF login and SQL cache write/read/remove.");
}

sealed class MarshContext(DbContextOptions<MarshContext> options) : DbContext(options);
// endsnippet:complete
