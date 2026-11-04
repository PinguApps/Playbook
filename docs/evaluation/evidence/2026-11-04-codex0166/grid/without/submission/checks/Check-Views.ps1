param([string]$BaseUrl = 'http://127.0.0.1:5187')
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
$script:requests = 0
$script:links = 0

function Assert($condition, [string]$message) {
    if (-not $condition) { throw $message }
}

function Read-Query([string]$url) {
    $result = @{}
    foreach ($part in ($url -split '\?', 2)[1] -split '&') {
        if (-not $part) { continue }
        $pair = $part -split '=', 2
        $result[[Uri]::UnescapeDataString($pair[0])] = [Uri]::UnescapeDataString($pair[1].Replace('+', ' '))
    }
    return $result
}

function Check-View([string]$url, [hashtable]$expected, [hashtable]$state) {
    $html = (Invoke-WebRequest -UseBasicParsing ($BaseUrl + $url)).Content
    $script:requests++
    Assert ($html -notmatch '<script|onclick|blazor:') "Unexpected interactivity: $url"
    foreach ($id in @('north', 'south')) {
        $section = [regex]::Match($html, ('(?s)<section id="' + $id + '">(.*?)</section>')).Groups[1].Value
        Assert ($section.Length -gt 0) "Missing section $id"
        $actual = @([regex]::Matches($section, '<tr><td>(.*?)</td><td>(.*?)</td></tr>') | ForEach-Object { $_.Groups[1].Value })
        Assert (($actual -join ',') -eq ($expected[$id] -join ',')) "Wrong rows for $id at $url : $actual"
        foreach ($link in [regex]::Matches($section, '<a href="([^"]+)"([^>]*)>(.*?)</a>')) {
            $script:links++
            $target = [Net.WebUtility]::HtmlDecode($link.Groups[1].Value)
            $query = Read-Query $target
            $other = if ($id -eq 'north') { 'south' } else { 'north' }
            foreach ($key in @('filter', "$other.page", "$other.sort", "$other.direction")) {
                $value = $query[$key]
                if ($null -eq $value -and $key -ne 'filter') { $value = $defaults[$key] }
                Assert ($value -ceq $state[$key]) "Link lost $key at $url"
            }
            if ($link.Groups[2].Value -match 'Sort (code|units) (ascending|descending)') {
                $sort = $Matches[1]
                $direction = if ($Matches[2] -eq 'ascending') { 'asc' } else { 'desc' }
                Assert ($query["$id.page"] -eq $state["$id.page"]) "Sorting reset page at $url"
                Assert ($query["$id.sort"] -eq $sort -and $query["$id.direction"] -eq $direction) "Wrong sort link"
            }
            else {
                Assert ($query["$id.sort"] -eq $state["$id.sort"] -and $query["$id.direction"] -eq $state["$id.direction"]) "Paging reset sorting"
            }
        }
    }
    return $html
}

$defaults = @{ 'north.page' = '1'; 'north.sort' = 'code'; 'north.direction' = 'asc'; 'south.page' = '1'; 'south.sort' = 'code'; 'south.direction' = 'asc' }
$null = Check-View '/' @{ north = @('N1', 'N2'); south = @('S1', 'S2') } $defaults
$orders = @{
    north = @{ 'code.asc' = @('N1','N2','N3','N4'); 'code.desc' = @('N4','N3','N2','N1'); 'units.asc' = @('N2','N4','N1','N3'); 'units.desc' = @('N3','N1','N4','N2') }
    south = @{ 'code.asc' = @('S1','S2','S3','S4'); 'code.desc' = @('S4','S3','S2','S1'); 'units.asc' = @('S3','S1','S4','S2'); 'units.desc' = @('S2','S4','S1','S3') }
}
foreach ($northOrder in $orders.north.Keys) {
    foreach ($southOrder in $orders.south.Keys) {
        foreach ($northPage in 1..2) {
            foreach ($southPage in 1..2) {
                $state = @{ filter = 'open water'; 'north.page' = "$northPage"; 'north.sort' = $northOrder.Split('.')[0]; 'north.direction' = $northOrder.Split('.')[1]; 'south.page' = "$southPage"; 'south.sort' = $southOrder.Split('.')[0]; 'south.direction' = $southOrder.Split('.')[1] }
                $url = '/?' + (($state.GetEnumerator() | Sort-Object Key | ForEach-Object { $_.Key + '=' + [Uri]::EscapeDataString($_.Value) }) -join '&')
                $expected = @{ north = @($orders.north[$northOrder] | Select-Object -Skip (($northPage - 1) * 2) -First 2); south = @($orders.south[$southOrder] | Select-Object -Skip (($southPage - 1) * 2) -First 2) }
                $first = Check-View $url $expected $state
                # Re-request the same address to verify a fresh request restores both views.
                $again = Check-View $url $expected $state
                Assert ($first -ceq $again) "Fresh request changed the view"
            }
        }
    }
}
$last = $defaults.Clone()
$last['north.page'] = '3'
$last['south.page'] = '3'
$null = Check-View '/?north.page=3&south.page=3' @{ north = @('NX'); south = @('SX') } $last
$empty = $defaults.Clone()
$empty['filter'] = ' open water '
$null = Check-View '/?filter=%20open%20water%20' @{ north = @(); south = @() } $empty
$closed = $defaults.Clone()
$closed['filter'] = 'closed'
$null = Check-View '/?filter=closed' @{ north = @('NX'); south = @('SX') } $closed
Write-Output "PASS: $script:requests HTTP requests; $script:links links checked. Defaults, all sort/page combinations, independent state, filter spaces, fresh requests, last/empty pages."
