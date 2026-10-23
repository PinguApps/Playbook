[CmdletBinding()]
param([string]$BodyPath='artifacts/pin795/inspected-recipe.json',[string]$OutputPath='artifacts/pin795/standalone-build.json')
$ErrorActionPreference='Stop'
$PSNativeCommandUseErrorActionPreference=$true
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'
$body=(Get-Content -LiteralPath $BodyPath -Raw|ConvertFrom-Json).markdown
$blocks=[regex]::Matches($body,'(?ms)^```[^\n]*\n(.*?)^```$')
$files=@(
 'net10.0/Host/Directory.Packages.props','net10.0/Host/Host.csproj','net10.0/Client/Client.csproj',
 'net10.0/Client/Chrome.razor','net10.0/Host/Server.razor','net10.0/Client/BrowserChrome.razor',
 'net11.0/Host/Directory.Packages.props','net11.0/Host/Host.csproj','net11.0/Client/Client.csproj',
 'net11.0/Client/Chrome.razor','net11.0/Host/Server.razor','net11.0/Client/BrowserChrome.razor',
 'net11.0/Host/Program.cs','net11.0/Client/Program.cs','net11.0/Host/App.razor',
 'net11.0/Host/Routes.razor','net11.0/Host/Browser.razor','net11.0/Host/wwwroot/chrome-start.js'
)
if($blocks.Count -ne $files.Count){throw 'Incomplete served implementation'}
$standalone=Join-Path ([IO.Path]::GetTempPath()) ('playbook-pin795-standalone-'+[guid]::NewGuid().ToString('N'))
for($n=0;$n -lt $files.Count;$n++){
 $path=Join-Path $standalone $files[$n]
 [IO.Directory]::CreateDirectory((Split-Path $path))|Out-Null
 [IO.File]::WriteAllText($path,$blocks[$n].Groups[1].Value)
}
foreach($file in @('Host/Program.cs','Client/Program.cs','Host/App.razor','Host/Routes.razor','Host/Browser.razor','Host/wwwroot/chrome-start.js')){
 $destination=Join-Path $standalone "net10.0/$file"
 [IO.Directory]::CreateDirectory((Split-Path $destination))|Out-Null
 Copy-Item -LiteralPath (Join-Path $standalone "net11.0/$file") -Destination $destination
}
foreach($v in @(10,11)){
 Copy-Item -LiteralPath (Join-Path $standalone "net$v.0/Host/Directory.Packages.props") -Destination (Join-Path $standalone "net$v.0/Client/Directory.Packages.props")
 $sdk=if($v -eq 10){'10.0.402'}else{'11.0.100-rc.2.26504.105'}
 if(-not $body.Contains($sdk) -or -not $body.Contains('rollForward disable')){throw 'SDK setup missing from body'}
 foreach($part in @('Host','Client')){
  [IO.File]::WriteAllText((Join-Path $standalone "net$v.0/$part/global.json"),(@{sdk=@{version=$sdk;rollForward='disable';allowPrerelease=($v -eq 11)}}|ConvertTo-Json -Depth 5)+"`n")
 }
}
$logs=[ordered]@{}
foreach($v in @(10,11)){
 $dotnet=if($v -eq 10){'C:\Program Files\dotnet\dotnet.exe'}else{'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe'}
 Push-Location (Join-Path $standalone "net$v.0/Host")
 try{
  $restore=(& $dotnet restore Host.csproj|Out-String)
  $build=(& $dotnet build Host.csproj -c Release --no-restore|Out-String)
  $logs["net$v.0"]=@{sdk=(& $dotnet --version|Out-String).Trim();command="$dotnet restore Host.csproj; $dotnet build Host.csproj -c Release --no-restore";output=$restore+$build}
  Write-Host $build
 }finally{Pop-Location}
}
$hashes=[ordered]@{}
foreach($file in Get-ChildItem $standalone -Recurse -File|Sort-Object FullName){
 if($file.FullName -match '\\obj\\'){continue}
 $hashes[[IO.Path]::GetRelativePath($standalone,$file.FullName).Replace('\','/')]=(Get-FileHash $file.FullName).Hash.ToLowerInvariant()
}
$record=@{kind='standalone-reconstruction-from-real-stdio-Markdown';directory=$standalone;recordedAt=[DateTimeOffset]::UtcNow.ToString('O');bodySha256=[Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($body)));verifierSha256=(Get-FileHash $PSCommandPath).Hash.ToLowerInvariant();logs=$logs;files=$hashes;scope='All implementation files come from expanded real stdio code fences, shared-file and literal SDK instructions. No repository source, parent configuration or lock files copied. Actual matching SDK builds Host and Client for both releases. Real HTTP/browser checks are separate.'}
[IO.File]::WriteAllText([IO.Path]::GetFullPath($OutputPath),($record|ConvertTo-Json -Depth 20).Replace("`r`n","`n")+"`n")
Write-Host "PASS reconstructed both release pairs at $standalone"
