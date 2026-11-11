#Requires -Version 7.0
[CmdletBinding()]
param([string]$Dotnet = 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$output = Join-Path $root 'artifacts/indexer-jump-contracts'
New-Item -ItemType Directory -Force $output | Out-Null
$before = @{}
foreach ($scope in 'Process','User','Machine') { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
try {
    $hostRoot = Split-Path $Dotnet
    $compiler = Join-Path $hostRoot 'sdk/11.0.100/Roslyn/bincore/csc.dll'
    $refs = @(Get-ChildItem (Join-Path $hostRoot 'packs/Microsoft.NETCore.App.Ref/11.0.0/ref/net11.0') -Filter '*.dll' | ForEach-Object { '-r:' + $_.FullName })
    function Probe([string]$Name, [string]$Source, [bool]$Valid, [string]$Diagnostic) {
        $file = Join-Path $output "$Name.cs"
        [IO.File]::WriteAllText($file, $Source)
        $result = & $Dotnet $compiler -nologo -target:library -langversion:15.0 "-out:$output/$Name.dll" @refs $file 2>&1 | Out-String
        if (($LASTEXITCODE -eq 0) -ne $Valid -or ($Diagnostic -and $result -notmatch $Diagnostic)) { throw "$Name unexpected result: $result" }
        Write-Host "PASS $Name $result"
    }
    $extension = 'public class Shelf {} public static class Access { extension(Shelf s) { public int this[int i] => i; } } '
    Probe 'ExpressionTree' ($extension + 'class Use { System.Linq.Expressions.Expression<System.Func<Shelf,int>> x = s => s[0]; }') $false 'CS9296'
    $generic = 'public static class Access { extension<T>(int s) { public T this[int i, T key] { set {} } } } '
    Probe 'AssignmentInferenceRejected' ($generic + 'class Use { void Run() { int shelf = 0; shelf[0, null] = new object(); } }') $false 'CS'
    Probe 'AssignmentInferenceValid' ($generic + 'class Use { void Run() { int shelf = 0; shelf[0, new object()] = null; Access.set_Item(shelf, 0, null, new object()); } }') $true
    Probe 'InstancePrecedence' 'class Shelf { public int Length => 3; public int this[int i] => i; } static class Access { extension(Shelf s) { public int this[System.Index i] => -1; } } class Use { int Read() => new Shelf()[^1]; }' $true
    Probe 'ArrayRejected' 'static class Access { extension(int[] s) { public int this[string i] => 1; } } class Use { int Read(int[] s) => s["bay"]; }' $false 'CS'
    Probe 'StringRejected' 'static class Access { extension(string s) { public int this[System.DateTime i] => 1; } } class Use { int Read(string s) => s[System.DateTime.Now]; }' $false 'CS'
    Probe 'BaseRejected' ($extension + 'class Child : Shelf { int Read() => base[0]; }') $false 'CS'
    Probe 'ValidLabels' 'class Use { void Run() { batches: for(int i=0;i<3;i++) { for(int j=0;j<2;j++) { if(j==0) continue batches; break batches; } } } }' $true
    Probe 'DuplicateLabels' 'class Use { void Run() { batch: while(true) { batch: while(true) { break batch; } } } }' $false 'CS0158'
    Probe 'BlockContinue' 'class Use { void Run() { batch: { while(true) { continue batch; } } } }' $false 'CS9394'
    Probe 'ImmediateLabel' 'class Use { void Run() { first: second: while(true) { continue first; } } }' $false 'CS9394'
    $fixture = Join-Path $output 'analyzer'
    New-Item -ItemType Directory -Force $fixture | Out-Null
    [IO.File]::WriteAllText((Join-Path $fixture 'Directory.Build.props'), '<Project />')
    [IO.File]::WriteAllText((Join-Path $fixture 'Directory.Build.targets'), '<Project />')
    [IO.File]::WriteAllText((Join-Path $fixture 'Fixture.csproj'), '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net11.0</TargetFramework><LangVersion>15.0</LangVersion><OutputType>Library</OutputType><EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild><TreatWarningsAsErrors>false</TreatWarningsAsErrors></PropertyGroup></Project>')
    [IO.File]::WriteAllText((Join-Path $fixture '.editorconfig'), "root = true`n[*.cs]`ncsharp_style_prefer_labeled_jump_statements = true`ndotnet_diagnostic.IDE0410.severity = warning`n")
    [IO.File]::WriteAllText((Join-Path $fixture 'Fixture.cs'), 'class ArchiveScan { public void Scan() { for(int batch=0;batch<3;batch++) { for(int slot=0;slot<4;slot++) { if(batch+slot==3) goto finished; } } finished: System.Console.WriteLine("finished"); } }')
    $result = & $Dotnet build (Join-Path $fixture 'Fixture.csproj') -c Release --nologo 2>&1 | Out-String
    if ($LASTEXITCODE -ne 0) { throw $result }
    Write-Host $result
    if ($result -notmatch 'warning IDE0410') { throw 'Expected IDE0410 build diagnostic missing.' }
    Write-Host 'PASS: actual SDK IDE0410 build diagnostic'
    foreach ($scope in 'Process','User','Machine') {
        if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "PATH changed: $scope" }
    }
} finally { $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous }
