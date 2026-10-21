#!/usr/bin/env dotnet
// snippet:reference
#:property TargetFramework=net11.0
#:property LangVersion=15.0
#:property PublishAot=false
#:property NuGetLockFilePath=dll-route.lock.json
#:include RackLibrary/bin/Release/net11.0/RackLibrary.dll

Console.WriteLine($"free={Depot.Rack.Free(12, 4)}");
// endsnippet:reference
