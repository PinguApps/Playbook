[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Seed,
    [Parameter(Mandatory)][string]$Playbook,
    [string]$Sdk = '11.0.100-rc.2.26504.105'
)

# Dedicated, owner-authorized verification repository only. Never a product checkout.
$ErrorActionPreference = 'Stop'
$seedPath = (Resolve-Path -LiteralPath $Seed).Path
$toolPath = (Resolve-Path -LiteralPath $Playbook).Path.Replace('\', '/')
$git = (Get-Command git -CommandType Application | Select-Object -First 1).Source
if (-not (Test-Path -LiteralPath (Join-Path $seedPath '.codex/config.toml'))) { throw 'Prepare the dedicated T3 verification seed first.' }
if (& $git -C $seedPath status --porcelain) { throw 'Preserve seed changes before preparing a checkpoint.' }
if ($LASTEXITCODE -ne 0) { throw 'Could not inspect seed.' }
if (Test-Path -LiteralPath (Join-Path $seedPath 'consumer')) { throw 'Consumer already exists; use a clean seed revision.' }
$tables = @(Get-Content -LiteralPath (Join-Path $seedPath '.codex/config.toml') | Where-Object { $_ -match '^\s*\[' })
if ($tables.Count -ne 1 -or $tables[0].Trim() -ne '[mcp_servers.playbook]') {
    throw 'Preserve other provider configuration; this helper only replaces the dedicated Playbook connection.'
}

@"
[mcp_servers.playbook]
command = '$toolPath'
args = ['mcp']
required = true
startup_timeout_sec = 30
"@ | Set-Content -LiteralPath (Join-Path $seedPath '.codex/config.toml')

@'
# Verification consumer

Use only this checkout. Keep PATH and persistent environment variables unchanged.
Use absolute executable paths. Set DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0 in every
.NET child process; disable workload update notifications. No installers,
global configuration edits, commits, pushes, PRs or delegated work are needed.

Before applicable .NET implementation work, consult Playbook. Inspect the
application's target frameworks, project/build files and global.json; choose
its target release, not its SDK version. Supply targetVersion on every
list_recipes page and get_recipe call; calls share no target state. Use latest
deliberately for exploration or an unknown target: it means the newest installed
catalogue target and does not establish compatibility. Discover relevant recipes,
retrieve full detail, then check each route's requirements and stability against
the application before applying and verifying guidance. Primary and optional
routes are separate choices. Exact evidence baselines are test records, not
required installation versions; RC evidence is not GA verification. Check
coverage: an empty result does not prove a capability is unavailable.

When discovery is empty and offers a suggested retry, first retry with the
original target selection, areas, languages and includeShared, changing only
keywords or paging as suggested. If useful afterwards, make a separate narrower
search and explain which filters changed; distinguish it from replaying the
suggestion. When no safe retry is offered, explain the unmatched combination
and coverage limits.

If the real Playbook MCP tools are unavailable, report that prerequisite and stop.
Do not substitute Inspector, a mock, source files or a shell implementation.
Finish within five execution minutes; preserve partial work and report a timeout.

The installed SDK executable is
C:\Users\pingu\.dotnet-playbook-11\dotnet.exe.
Build from consumer/ so its global.json applies. Run the resulting
consumer/bin/Debug/net10.0/Consumer.dll with
C:\Program Files\dotnet\dotnet.exe, which has the .NET 10 runtime.
'@ | Set-Content -LiteralPath (Join-Path $seedPath 'AGENTS.md')

$consumer = Join-Path $seedPath 'consumer'
New-Item -ItemType Directory -Path $consumer | Out-Null
@{ sdk = @{ version = $Sdk; rollForward = 'disable' } } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $consumer 'global.json')
# Explicit empty boundaries prevent inheritance from unrelated parent build files.
'<Project />' | Set-Content -LiteralPath (Join-Path $consumer 'Directory.Build.props')
'<Project />' | Set-Content -LiteralPath (Join-Path $consumer 'Directory.Build.targets')
'<Project />' | Set-Content -LiteralPath (Join-Path $consumer 'Directory.Packages.props')
@'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>14.0</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
</Project>
'@ | Set-Content -LiteralPath (Join-Path $consumer 'Consumer.csproj')
@'
var item = new StockLabel("  bay-a  ");
if (item.Code != "BAY-A") throw new Exception("Constructor normalization");
item.Code = " rack-b ";
if (item.Code != "RACK-B") throw new Exception("Assignment normalization");
try { item.Code = " "; throw new Exception("Blank accepted"); }
catch (ArgumentException) { }
try { _ = new StockLabel(" "); throw new Exception("Blank constructor accepted"); }
catch (ArgumentException) { }
if (item.Code != "RACK-B") throw new Exception("Invalid assignment mutated value");
Console.WriteLine("PASS: constructor, assignment, rejection and retained value");

public sealed class StockLabel
{
    private string code = "UNASSIGNED";
    public StockLabel(string code) => Code = code;
    public string Code
    {
        get => code;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            code = value.Trim().ToUpperInvariant();
        }
    }
}
'@ | Set-Content -LiteralPath (Join-Path $consumer 'Program.cs')
Add-Content -LiteralPath (Join-Path $seedPath '.gitignore') -Value "`n**/bin/`n**/obj/"
Write-Host 'Prepared consumer and companion instruction. Inspect, build/run, then commit the seed before launching.'
