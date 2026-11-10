#!/usr/bin/env dotnet
// snippet:complete
#:include Migrations/**/*.cs
#:include CurlewMigrations/**/*.cs
#:package Microsoft.EntityFrameworkCore.Sqlite@11.0.0
#:package Microsoft.EntityFrameworkCore.Design@11.0.0
#:property PublishAot=false
#:property TargetFramework=net11.0
#:property LangVersion=15.0
#:property RestorePackagesWithLockFile=true
#:property NuGetLockFilePath=packages.lock.json
using Microsoft.EntityFrameworkCore;
using var db = new CurlewFile.Marsh();
if (args.Contains("--check")) {
    db.Nests.Add(new CurlewFile.Nest { Code = "CUR-7" }); db.SaveChanges();
    if (db.Nests.AsNoTracking().Count(n => n.Code == "CUR-7") != 1) throw new Exception("Persistence failed");
    Console.WriteLine("PASS migrated schema and persisted file-app row");
}
namespace CurlewFile { public sealed class Marsh : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite("Data Source=curlew.db");
    public DbSet<Nest> Nests => Set<Nest>();
}
public sealed class Nest { public int Id { get; set; } public string Code { get; set; } = ""; }

}




// endsnippet:complete
