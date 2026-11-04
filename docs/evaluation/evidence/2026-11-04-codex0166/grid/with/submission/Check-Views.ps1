param([string]$BaseUrl = 'http://127.0.0.1:5087/')
$ErrorActionPreference = 'Stop'
$script:checks = 0
function Get-Html([string]$url) {
    (Invoke-WebRequest -UseBasicParsing $url).Content
}
function Section([string]$html, [string]$id) {
    [regex]::Match($html, ('(?s)<section id="' + $id + '">.*?</section>')).Value
}
function Assert-Rows([string]$html, [string]$id, [string]$expected) {
    $body = [regex]::Match((Section $html $id), '(?s)<tbody[^>]*>(.*?)</tbody>').Groups[1].Value
    $actual = ([regex]::Matches($body, '<td[^>]*>([NS][1-4X])</td>') | ForEach-Object { $_.Groups[1].Value }) -join ','
    if ($actual -ne $expected) { throw "$id rows: expected $expected; got $actual" }
    $script:checks++
}
function Expected([string]$id, [string]$sort, [string]$direction, [int]$page) {
    $codes = if ($sort -eq 'code') { @(1,2,3,4) } elseif ($id -eq 'north') { @(2,4,1,3) } else { @(3,1,4,2) }
    if ($direction -eq 'desc') { [array]::Reverse($codes) }
    $prefix = if ($id -eq 'north') { 'N' } else { 'S' }
    ($codes | Select-Object -Skip (($page - 1) * 2) -First 2 | ForEach-Object { "$prefix$_" }) -join ','
}
$default = Get-Html $BaseUrl
Assert-Rows $default north 'N1,N2'
Assert-Rows $default south 'S1,S2'
if ($default -match '<script|<button|blazor.web.js') { throw 'Unexpected interactive markup' }
foreach ($ns in @('code','units')) { foreach ($nd in @('asc','desc')) { foreach ($np in @(1,2)) {
    foreach ($ss in @('code','units')) { foreach ($sd in @('asc','desc')) { foreach ($sp in @(1,2)) {
        $url = "${BaseUrl}?filter=open%20water&north.page=$np&north.sort=$ns&north.direction=$nd&south.page=$sp&south.sort=$ss&south.direction=$sd"
        $html = Get-Html $url
        Assert-Rows $html north (Expected north $ns $nd $np)
        Assert-Rows $html south (Expected south $ss $sd $sp)
        if ($html -notmatch 'Filter: open water') { throw 'Filter lost its space' }
    } } }
} } }
# Follow every sort and page link from a bookmark where both grids have state.
$bookmark = "${BaseUrl}?filter=open%20water&north.page=2&north.sort=units&north.direction=desc&south.page=2&south.sort=code&south.direction=desc"
$html = Get-Html $bookmark
foreach ($id in @('north','south')) {
    $other = if ($id -eq 'north') { 'south' } else { 'north' }
    foreach ($link in [regex]::Matches((Section $html $id), '<a\b[^>]*href="([^"]+)"[^>]*>')) {
        $url = [System.Net.WebUtility]::HtmlDecode($link.Groups[1].Value)
        $query = @{}
        foreach ($pair in ([uri]$url).Query.TrimStart('?').Split('&')) {
            $parts = $pair.Split('=', 2)
            $query[[uri]::UnescapeDataString($parts[0])] = [uri]::UnescapeDataString($parts[1])
        }
        if ($query['filter'] -ne 'open water' -or $query["$other.page"] -ne '2' -or $query["$other.direction"] -ne 'desc') { throw "State lost in $url" }
        $otherSort = if ($other -eq 'north') { 'units' } else { 'code' }
        if ($query["$other.sort"] -ne $otherSort) { throw "Other sort lost in $url" }
        if ($link.Value -notmatch 'class="go-' -and $query["$id.page"] -ne '2') { throw 'Sort reset the page' }
        $page = if ($query.ContainsKey("$id.page")) { [int]$query["$id.page"] } else { 1 }
        $linked = Get-Html $url
        Assert-Rows $linked $id (Expected $id $query["$id.sort"] $query["$id.direction"] $page)
        Assert-Rows $linked $other (Expected $other $otherSort desc 2)
    }
}
# Repeat saved addresses, as on refresh or return to a history entry.
foreach ($url in @($bookmark, $BaseUrl, $bookmark)) {
    $html = Get-Html $url
    if ($url -eq $bookmark) { Assert-Rows $html north 'N4,N2'; Assert-Rows $html south 'S2,S1' }
    else { Assert-Rows $html north 'N1,N2'; Assert-Rows $html south 'S1,S2' }
}
$closed = Get-Html "${BaseUrl}?filter=closed"
Assert-Rows $closed north 'NX'
Assert-Rows $closed south 'SX'
$empty = Get-Html "${BaseUrl}?filter=missing"
Assert-Rows $empty north ''
Assert-Rows $empty south ''
Write-Output "Passed $script:checks row checks, 64 combined views, 16 rendered links, default/filter and saved-address checks."
