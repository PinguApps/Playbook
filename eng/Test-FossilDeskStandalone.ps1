$ErrorActionPreference='Stop'
$PSNativeCommandUseErrorActionPreference=$true
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'
$root=(Get-Location).Path
$body=(Get-Content artifacts/pin792/inspected-recipe.json -Raw|ConvertFrom-Json).markdown
$blocks=[regex]::Matches($body,'(?ms)^```[^\n]*\n(.*?)^```$')
$files=@(
 'net10.0/FossilDesk/FossilDesk.csproj','net10.0/FossilDesk/Directory.Packages.props',
 'net10.0/FossilDesk/Program.cs','net10.0/FossilDesk/Desk.razor',
 'net10.0/FossilDesk/wwwroot/fossil-client.js','net10.0/FossilDesk/wwwroot/fossil-score.js',
 'net11.0/FossilDesk/Directory.Packages.props','net11.0/FossilDesk/FossilDesk.csproj',
 'net11.0/FossilWorker/FossilWorker.csproj','net11.0/FossilWorker/WorkerMethods.cs',
 'net11.0/FossilDesk/Program.cs','net11.0/FossilDesk/Desk.razor','net11.0/FossilDesk/WorkerProbe.cs',
 'net11.0/FossilDesk/wwwroot/index.html','net11.0/FossilDesk/wwwroot/fossil-observe.js',
 'net11.0/FossilWorker/WebWorkerClient.cs','net11.0/FossilWorker/wwwroot/dotnet-web-worker-client.js',
 'net11.0/FossilWorker/wwwroot/dotnet-web-worker.js'
)
if($blocks.Count -ne $files.Count){throw 'Incomplete served body'}
$standalone=Join-Path ([IO.Path]::GetTempPath()) ('playbook-pin792-standalone-'+[guid]::NewGuid().ToString('N'))
for($n=0;$n -lt $files.Count;$n++){
 $path=Join-Path $standalone $files[$n]
 [IO.Directory]::CreateDirectory((Split-Path $path))|Out-Null
 [IO.File]::WriteAllText($path,$blocks[$n].Groups[1].Value)
}
foreach($f in @('wwwroot/index.html','wwwroot/fossil-observe.js')){
 Copy-Item (Join-Path $standalone "net11.0/FossilDesk/$f") (Join-Path $standalone "net10.0/FossilDesk/$f")
}
Copy-Item (Join-Path $standalone 'net11.0/FossilDesk/Directory.Packages.props') (Join-Path $standalone 'net11.0/FossilWorker/Directory.Packages.props')
foreach($case in @(@('net10.0/FossilDesk','10.0.402'),@('net11.0/FossilDesk','11.0.100-rc.2.26504.105'),@('net11.0/FossilWorker','11.0.100-rc.2.26504.105'))){
 if(-not $body.Contains($case[1])){throw 'SDK missing from served instructions'}
 [IO.File]::WriteAllText((Join-Path $standalone "$($case[0])/global.json"),(@{sdk=@{version=$case[1];rollForward='disable';allowPrerelease=$case[0].StartsWith('net11')}}|ConvertTo-Json)+"`n")
}
$logs=@{}
foreach($target in @('net10.0','net11.0')){
 $dotnet=if($target -eq 'net10.0'){'C:\Program Files\dotnet\dotnet.exe'}else{'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe'}
 Push-Location (Join-Path $standalone "$target/FossilDesk")
 try{
  $log=(& $dotnet restore ./FossilDesk.csproj|Out-String)+(& $dotnet build ./FossilDesk.csproj -c Release --no-restore|Out-String)
  $logs[$target]=@{sdk=(& $dotnet --version|Out-String).Trim();output=$log;command="$dotnet restore FossilDesk.csproj; $dotnet build FossilDesk.csproj -c Release --no-restore"}
  Write-Host $log
 }finally{Pop-Location}
}
Push-Location (Join-Path $standalone 'net11.0/FossilWorker')
try { $logs.worker=(& 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe' build ./FossilWorker.csproj -c Release --no-restore|Out-String) } finally {Pop-Location}
$hashes=[ordered]@{}
foreach($file in Get-ChildItem $standalone -Recurse -File|Sort-Object FullName){
 if($file.FullName -match '\\obj\\'){continue}
 $hashes[[IO.Path]::GetRelativePath($standalone,$file.FullName).Replace('\','/')]=(Get-FileHash $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
}
@{kind='standalone-reconstruction-from-real-stdio-detail';directory=$standalone;recordedAt=[DateTimeOffset]::UtcNow.ToString('O');bodySha256=[Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($body)));logs=$logs;files=$hashes;scope='Every implementation file comes from served Markdown code fences, shared-file instructions and literal SDK instructions. No repository source, parent imports or lock files copied; actual matching SDK builds. Separate real browser execution follows.'}|ConvertTo-Json -Depth 20|Set-Content artifacts/pin792/standalone-build.json
