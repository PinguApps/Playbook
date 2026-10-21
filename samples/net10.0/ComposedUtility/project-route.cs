#!/usr/bin/env dotnet
// snippet:reference
#:property TargetFramework=net10.0
#:property LangVersion=14.0
#:property PublishAot=false
#:property NuGetLockFilePath=project-route.lock.json
#:project RackLibrary/RackLibrary.csproj

Console.WriteLine($"free={Depot.Rack.Free(12, 4)}");
// endsnippet:reference
