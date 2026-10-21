#!/usr/bin/env dotnet
// snippet:compose
#:property TargetFramework=net10.0
#:property LangVersion=14.0
#:property PublishAot=false
#:property ManagePackageVersionsCentrally=false
#:property RestorePackagesWithLockFile=true
#:property NuGetLockFilePath=packages.lock.json
#:property Company="Estuary Depot Tools"
#:include Rack.cs
#:package System.CommandLine@2.0.1 Aliases=DepotCli

extern alias DepotCli;
using Depot;
using System.Reflection;

DepotCli::System.CommandLine.Option<int> reserve = new("--reserve") { DefaultValueFactory = _ => 2 };
DepotCli::System.CommandLine.RootCommand command = new("Calculate free slots in the depot.");
command.Options.Add(reserve);
int held = command.Parse(args).GetValue(reserve);
Console.WriteLine($"free={Rack.Free(12, held)};company={Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyCompanyAttribute>()?.Company}");
// endsnippet:compose
