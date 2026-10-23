[CmdletBinding()]
param([Parameter(Mandatory)][uri]$BaseUri, [string]$EvidencePath)
$ErrorActionPreference = 'Stop'
$passed = [Collections.Generic.List[string]]::new()
function Read-Page([string]$url) { (Invoke-WebRequest -Uri $url).Content }
function Section([string]$html, [string]$grid) {
    $match = [regex]::Match($html, "<section id=`"$($grid)_`">(.*?)</section>", 'Singleline')
    if (-not $match.Success) { throw "Missing grid $grid" }
    $match.Groups[1].Value
}
function Codes([string]$html, [string]$grid) {
    $body = [regex]::Match((Section $html $grid), '<tbody[^>]*>(.*?)</tbody>', 'Singleline').Groups[1].Value
    ([regex]::Matches($body, '<tr[^>]*>\s*<td[^>]*>([^<]+)</td>') | ForEach-Object { $_.Groups[1].Value }) -join ','
}
function Check([bool]$condition, [string]$message) {
    if (-not $condition) { throw $message }
    $passed.Add($message)
}
function Link([string]$section, [string]$class) {
    [Net.WebUtility]::HtmlDecode([regex]::Match($section, "<a class=`"$class`" href=`"([^`"]+)`"").Groups[1].Value)
}
$bookmark = 'static?scope=open&repairs_sort=priority&repairs_direction=desc&repairs_page=2&loans_sort=code&loans_direction=desc&loans_page=3'
$url = [uri]::new($BaseUri, $bookmark).AbsoluteUri
$html = Read-Page $url
Check ((Codes $html repairs) -ceq 'R-E,R-F' -and (Codes $html loans) -ceq 'L-B,L-A') 'Both bookmarked sorted pages are restored by HTTP SSR'
Check ((Read-Page $url) -match 'R-E') 'Bookmark can be requested again'
$renamed = Read-Page ($url + '&labels=alt')
Check ($renamed.Contains('Ticket reference') -and $renamed.Contains('Work order') -and (Codes $renamed repairs) -ceq 'R-E,R-F' -and (Codes $renamed loans) -ceq 'L-B,L-A') 'Changed visible headings preserve stable URL identity'
$next = Link (Section $html repairs) 'go-next'
Check ($next.Contains('loans_page=3') -and $next.Contains('scope=open')) 'Repair link retains other namespace and unrelated query'
$after = Read-Page $next
Check ((Codes $after repairs) -ceq 'R-D,R-B' -and (Codes $after loans) -ceq 'L-B,L-A') 'Actual repair link changes only repair rows'
$previous = Link (Section $after loans) 'go-previous'
$after = Read-Page $previous
Check ((Codes $after repairs) -ceq 'R-D,R-B' -and (Codes $after loans) -ceq 'L-D,L-C') 'Actual loan link changes only loan rows'
$sort = Link (Section $after repairs) 'col-title'
$after = Read-Page $sort
Check ((Codes $after repairs) -ceq 'R-E,R-F' -and (Codes $after loans) -ceq 'L-D,L-C') 'Actual sort link changes only repair ordering'
$loans = Section $html loans
Check ($loans -match '<a class="go-next"[^>]*aria-disabled="true"[^>]*tabindex="-1"') 'Disabled pagination uses anchor aria-disabled and tabindex'
$first = Read-Page ([uri]::new($BaseUri, 'static?scope=open').AbsoluteUri)
$back = Link (Section $first repairs) 'go-previous'
Check (-not $back.Contains('repairs_page=')) 'First page omits its query key'
Check ((Codes $first repairs) -ceq 'R-A,R-B' -and (Codes $first loans) -ceq 'L-A,L-B') 'Missing parameters restore defaults independently'
$defaultSort = Read-Page (Link (Section $first repairs) 'col-title')
Check ((Codes $defaultSort repairs) -ceq 'R-F,R-E' -and (Codes $defaultSort loans) -ceq 'L-A,L-B') 'Default ascending header toggles to descending without changing the other grid'
foreach ($page in @('0', '-2', 'bad', '999')) {
    $edge = Read-Page ([uri]::new($BaseUri, "static?repairs_page=$page&loans_page=2").AbsoluteUri)
    $expected = if ($page -eq '999') { 'R-E,R-F' } else { 'R-A,R-B' }
    Check ((Codes $edge repairs) -ceq $expected -and (Codes $edge loans) -ceq 'L-C,L-D') "Page $page resolves without changing other grid"
}
$report = [ordered]@{ kind = 'execution'; recordedAt = [DateTimeOffset]::UtcNow.ToString('O'); baseUri = $BaseUri.AbsoluteUri; bookmark = $url; passed = $passed.ToArray() }
if ($EvidencePath) { $report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $EvidencePath }
Write-Host "PASS: $($passed.Count) actual HTTP two-grid checks at $BaseUri"
