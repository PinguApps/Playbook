#!/usr/bin/env dotnet
// snippet:complete
#:package Microsoft.EntityFrameworkCore.Sqlite@11.0.0-rc.2.26504.105
#:package Microsoft.EntityFrameworkCore.Design@11.0.0-rc.2.26504.105
#:property PublishAot=false
#:property TargetFramework=net11.0
#:property LangVersion=15.0
#:property RestorePackagesWithLockFile=true
#:property NuGetLockFilePath=packages.lock.json
#:project ../CurlewFileModel/CurlewFileModel.csproj

using Microsoft.EntityFrameworkCore;
using var db = new CurlewFileModel.SurveyContext();
if (args.Contains("--check")) {
    db.Nests.Add(new CurlewFileModel.Nest { Code = "CUR-7" }); db.SaveChanges();
    if (db.Nests.AsNoTracking().Count(n => n.Code == "CUR-7") != 1) throw new Exception("Persistence failed");
    Console.WriteLine("PASS migrated schema and persisted mixed startup row");
}


// endsnippet:complete
