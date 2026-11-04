param(
    [Parameter(Mandatory)][string] $Submission,
    [Parameter(Mandatory)][string] $Dotnet,
    [switch] $Reference
)
$ErrorActionPreference = 'Stop'
$workspace = Join-Path ([IO.Path]::GetTempPath()) ('grid-oracle-' + [guid]::NewGuid())
$server = $null
$failed = 0
$paths = @{}
foreach ($scope in 'User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
function Assert($claim, $condition) {
    if ($condition) { Write-Host "PASS: $claim" } else { Write-Host "FAIL: $claim"; $script:failed++ }
}
try {
    $root = (Resolve-Path -LiteralPath $Submission).Path
    Get-ChildItem -LiteralPath $root -Recurse -File -Force | ForEach-Object {
        $relative = [IO.Path]::GetRelativePath($root, $_.FullName)
        if ($relative -match '(^|[\\/])(bin|obj|\.git)([\\/]|$)') { return }
        $destination = Join-Path $workspace $relative
        New-Item -ItemType Directory -Force (Split-Path $destination) | Out-Null
        Copy-Item -LiteralPath $_.FullName -Destination $destination
    }
    if ($Reference) { Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Reference.razor') -Destination (Join-Path $workspace 'Shelf.razor') }
    $start = [Diagnostics.ProcessStartInfo]::new($Dotnet)
    $start.WorkingDirectory = $workspace
    $start.UseShellExecute = $false
    $start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
    $start.Environment['Logging__LogLevel__Default'] = 'Warning'
    $start.ArgumentList.Add('build')
    $start.ArgumentList.Add('-p:RestoreLockedMode=true')
    $build = [Diagnostics.Process]::Start($start)
    if (!$build.WaitForExit(120000)) { $build.Kill($true); throw 'Build timeout' }
    if ($build.ExitCode -ne 0) { throw 'Build failed' }
    $listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 0)
    $listener.Start(); $port = $listener.LocalEndpoint.Port; $listener.Stop()
    $start.ArgumentList.Clear()
    foreach ($arg in @('run','--no-build','--urls',"http://127.0.0.1:$port")) { $start.ArgumentList.Add($arg) }
    $server = [Diagnostics.Process]::Start($start)
    $base = "http://127.0.0.1:$port/"
    $ready = $false
    for ($i=0; $i -lt 40; $i++) {
        try { $null = Invoke-WebRequest $base -TimeoutSec 2; $ready = $true; break } catch { Start-Sleep -Milliseconds 250 }
    }
    if (!$ready) { throw 'Server did not become ready' }
    function Fetch($url) { (Invoke-WebRequest $url -TimeoutSec 10).Content }
    function Section($html, $id) { [regex]::Match($html, "(?s)<section\b[^>]*id=`"$id`"[^>]*>(.*?)</section>").Groups[1].Value }
    function Codes($html, $id) {
        $body = Section $html $id
        @([regex]::Matches($body, '<td\b[^>]*>\s*([NS][1-4X])\s*</td>') | ForEach-Object {$_.Groups[1].Value}) -join ','
    }
    function Anchor($html, $id, $key, $value) {
        foreach ($match in [regex]::Matches((Section $html $id), '<a\b[^>]*href="([^"]+)"[^>]*>')) {
            $href = [Net.WebUtility]::HtmlDecode($match.Groups[1].Value)
            $uri = [Uri]::new([Uri]$base, $href)
            if ($uri.Query -match "(?:[?&])$([regex]::Escape($key))=$value(?:&|$)") { return $uri.AbsoluteUri }
        }
        return $null
    }
    $bookmark = $base + '?filter=open%20water&north.page=2&north.sort=units&north.direction=asc&south.page=1&south.sort=code&south.direction=desc'
    $html = Fetch $bookmark
    Assert 'north bookmark restores units ascending page two' ((Codes $html north) -eq 'N1,N3')
    Assert 'south bookmark independently restores code descending' ((Codes $html south) -eq 'S4,S3')
    Assert 'refresh restores both tables and excludes closed rows' ((Fetch $bookmark) -match 'Filter: open water' -and (Codes (Fetch $bookmark) north) -eq 'N1,N3' -and $html -notmatch '>NX<|>SX<')
    foreach ($case in @(@('north','north.page','1','N2,N4','S4,S3'), @('south','south.page','2','N1,N3','S2,S1'), @('north','north.direction','desc','N4,N2','S4,S3'), @('south','south.direction','asc','N1,N3','S1,S2'))) {
        $url = Anchor $html $case[0] $case[1] $case[2]
        Assert "ordinary link exists: $($case[1])=$($case[2])" ($null -ne $url)
        if ($url) {
            $next = Fetch $url
            Assert "navigation preserves filter and independent rows: $($case[1])" ($next -match 'Filter: open water' -and (Codes $next north) -eq $case[3] -and (Codes $next south) -eq $case[4])
            Assert 'returning to bookmarked history entry restores state' ((Codes (Fetch $bookmark) north) -eq 'N1,N3' -and (Codes (Fetch $bookmark) south) -eq 'S4,S3')
        }
    }
    $default = Fetch ($base + '?filter=open%20water')
    Assert 'default view is page one code ascending' ((Codes $default north) -eq 'N1,N2' -and (Codes $default south) -eq 'S1,S2')
    Write-Host "Failed assertions: $failed"
    if ($failed) { throw 'Behavior oracle failed' }
} finally {
    if ($server -and !$server.HasExited) { $server.Kill($true); $server.WaitForExit() }
    foreach ($scope in 'User','Machine') {
        if ([Environment]::GetEnvironmentVariable('PATH', $scope) -cne $paths[$scope]) { Write-Error "Unexpected $scope PATH change" }
    }
    Write-Host "Disposable source retained at $workspace"
}
