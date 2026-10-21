#!/usr/bin/env dotnet
// snippet:compose
#:property TargetFramework=net10.0
#:property LangVersion=14.0
#:property PublishAot=false
#:property ManagePackageVersionsCentrally=false
#:property RestorePackagesWithLockFile=true
#:property NuGetLockFilePath=packages.lock.json
#:include Rack.cs
#:package System.CommandLine@2.0.1

using System.CommandLine;
using Depot;

Option<int> reserve = new("--reserve") { DefaultValueFactory = _ => 2 };
RootCommand command = new("Calculate free slots in the depot.");
command.Options.Add(reserve);
int held = command.Parse(args).GetValue(reserve);
Console.WriteLine($"free={Rack.Free(12, held)}");
// endsnippet:compose
