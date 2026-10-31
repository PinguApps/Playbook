using Microsoft.EntityFrameworkCore;

var connection = Environment.GetEnvironmentVariable("PLAYBOOK_SQL_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Run through eng/SqlServer.ps1 -Action Run.");
var options = new DbContextOptionsBuilder<DbContext>()
    .UseSqlServer(connection, sql => sql.UseCompatibilityLevel(170)).Options;
await using var database = new DbContext(options);
await database.Database.ExecuteSqlRawAsync("CREATE TABLE dbo.ReefReadings (Id int PRIMARY KEY, Label nvarchar(100) NOT NULL);");
#if NET11_0_OR_GREATER
await database.Database.ExecuteSqlAsync($"INSERT dbo.ReefReadings VALUES ({1}, {"coral"});");
#else
await database.Database.ExecuteSqlInterpolatedAsync($"INSERT dbo.ReefReadings VALUES ({1}, {"coral"});");
#endif
var label = await database.Database.SqlQuery<string>($"SELECT Label AS Value FROM dbo.ReefReadings WHERE Id = {1}").SingleAsync();
if (label != "coral") throw new InvalidOperationException("EF SQL read/write probe failed.");
Console.WriteLine($"PASS: {database.Database.ProviderName} {typeof(DbContext).Assembly.GetName().Version}; parameterized write/read over loopback.");
