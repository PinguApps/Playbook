[CmdletBinding()]
param([Parameter(Mandatory)][uri]$BaseUri, [string]$OutputPath)

$ErrorActionPreference = 'Stop'
$client = [Net.Http.HttpClient]::new()
$client.BaseAddress = $BaseUri
$results = [Collections.Generic.List[object]]::new()
function Assert-True([bool]$Value, [string]$Message) { if (-not $Value) { throw $Message } }
function Get-Journal([string]$Code) { Invoke-RestMethod ([uri]::new($BaseUri, "/control/$Code")) }
function Post-Static([string]$Code, [int]$Guests, [string]$Culture, [string]$Expected) {
  $url = [uri]::new($BaseUri, "/static?culture=$Culture&ui-culture=$Culture")
  $page = Invoke-WebRequest $url -SessionVariable formSession
  $token = [Net.WebUtility]::HtmlDecode([regex]::Match($page.Content, 'name="__RequestVerificationToken" value="([^"]+)"').Groups[1].Value)
  $response = Invoke-WebRequest $url -Method Post -WebSession $formSession -Body @{
    _handler = 'visit'; __RequestVerificationToken = $token; 'Model.Pass' = $Code; 'Model.Guests' = $Guests
  }
  $html = [Net.WebUtility]::HtmlDecode($response.Content)
  Assert-True ($html.Contains("<output id=`"result`">$Expected</output>")) "Wrong static form result: $html"
  return $html
}
try {
  foreach ($code in @('open-held', 'closed-held')) {
    $null = Invoke-WebRequest ([uri]::new($BaseUri, "/control/arm/$code")) -Method Post
    $body = [Net.Http.StringContent]::new("{`"pass`":`"$code`",`"guests`":2}", [Text.Encoding]::UTF8, 'application/json')
    try {
      $pending = $client.PostAsync('/visits', $body)
      $deadline = [DateTime]::UtcNow.AddSeconds(10)
      do { $before = Get-Journal $code; if ($before.entered) { break }; Start-Sleep -Milliseconds 20 } while ([DateTime]::UtcNow -lt $deadline)
      Assert-True ($before.entered -eq 1 -and $before.completed -eq 0 -and $before.accepted -eq 0 -and -not $pending.IsCompleted) 'Handler or request finished before the asynchronous rule.'
      $null = Invoke-WebRequest ([uri]::new($BaseUri, "/control/release/$code")) -Method Post
      $response = $pending.GetAwaiter().GetResult()
      $after = Get-Journal $code
      $expectedStatus = if ($code.StartsWith('open')) { 204 } else { 400 }
      Assert-True ([int]$response.StatusCode -eq $expectedStatus) 'Wrong awaited endpoint status.'
      Assert-True ($after.completed -eq 1 -and $after.accepted -eq $(if ($expectedStatus -eq 204) { 1 } else { 0 })) 'Handler execution does not match rule result.'
      $results.Add(@{ code = $code; before = $before; after = $after; status = [int]$response.StatusCode })
      $response.Dispose()
    }
    finally { $body.Dispose() }
  }
  $null = Post-Static 'open-form' 2 'en-GB' 'Accepted'
  $closed = Post-Static 'closed-form' 2 'en-GB' 'Rejected'
  Assert-True ($closed.Contains('This visit is closed.')) 'Async form error missing.'
  Assert-True ((Get-Journal 'closed-form').accepted -eq 0) 'Invalid form was accepted.'
  foreach ($culture in @('en-GB', 'fr-FR')) {
    $expected = if ($culture -eq 'fr-FR') { 'Taille du groupe : choisissez de 1 à 6 visiteurs.' } else { 'Party size: choose between 1 and 6 guests.' }
    $content = [Net.Http.StringContent]::new('{"pass":"open","guests":9}', [Text.Encoding]::UTF8, 'application/json')
    try {
      $response = $client.PostAsync("/visits?culture=$culture&ui-culture=$culture", $content).GetAwaiter().GetResult()
      $json = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult() | ConvertFrom-Json
      Assert-True ([int]$response.StatusCode -eq 400 -and $json.errors.Guests[0] -ceq $expected) 'Endpoint resource/formatter mismatch.'
      $html = Post-Static 'open-localized' 9 $culture 'Rejected'
      Assert-True ($html.Contains($expected)) 'Blazor resource/formatter mismatch.'
      $results.Add(@{ culture = $culture; endpointMessage = $json.errors.Guests[0]; formMessage = $expected; status = 400 })
      $response.Dispose()
    }
    finally { $content.Dispose() }
  }
  $report = @{ recordedAt = [DateTimeOffset]::UtcNow.ToString('O'); baseUri = "$BaseUri"; checks = $results; staticValid = 'Accepted'; staticInvalid = 'Rejected'; kind = 'real-loopback-http' }
  if ($OutputPath) { $report | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $OutputPath }
  Write-Host 'PASS: held valid/invalid endpoints await rules before handler execution; actual static Blazor forms reject closed visits; en-GB/fr-FR messages match with custom formatting and member-over-type/global resource precedence.'
}
finally { $client.Dispose() }
