$ErrorActionPreference = 'Stop'
$env:TEMP = 'C:/Users/pingu/AppData/Local/Temp/playbook-reservation-20261104/with/temp'
$env:TMP = $env:TEMP
$env:MSBuildEnableWorkloadResolver = 'false'
& 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe' exec 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/sdk/11.0.100-rc.3.26522.148/MSBuild.dll' ReservationEvaluation.csproj /t:Build /p:Configuration=Release /p:NuGetAudit=false /p:UseSharedCompilation=false /verbosity:minimal
exit $LASTEXITCODE
