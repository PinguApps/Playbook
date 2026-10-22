[CmdletBinding()]
param(
  [Parameter(Mandatory)][uri]$BaseUri,
  [switch]$Net11,
  [switch]$GlobalClientOptOut,
  [string]$OutputPath
)

$ErrorActionPreference = 'Stop'
$results = [Collections.Generic.List[object]]::new()
function Assert-Contains([string]$Text, [string]$Expected) {
  if (-not $Text.Contains($Expected, [StringComparison]::Ordinal)) { throw "Missing expected response text: $Expected" }
}
function Post-Form([string]$Path, [hashtable]$Values, [string]$Expected) {
  $url = [uri]::new($BaseUri, $Path)
  $page = Invoke-WebRequest $url -SessionVariable formSession
  $body = @{ _handler = $(if ($Path -like '/diagnostics/*') { 'undiscovered' } else { 'survey' }) }
  $body.__RequestVerificationToken = [Net.WebUtility]::HtmlDecode([regex]::Match($page.Content, 'name="__RequestVerificationToken" value="([^"]+)"').Groups[1].Value)
  foreach ($key in $Values.Keys) { $body[$key] = $Values[$key] }
  $response = Invoke-WebRequest $url -Method Post -WebSession $formSession -Body $body
  Assert-Contains $response.Content "<output id=`"result`">$Expected</output>"
  $results.Add(@{ path = $Path; expected = $Expected; status = [int]$response.StatusCode; values = $Values })
  return $response.Content
}
$valid = @{ 'Model.Lead' = 'Reef team'; 'Model.Contact.Email' = 'reef@example.test'; 'Model.Divers[0].Name' = 'Aster'; 'Model.Divers[1].Name' = 'Wren' }
foreach ($mode in @('enhanced', 'plain')) {
  $page = Invoke-WebRequest ([uri]::new($BaseUri, "/$mode"))
  if ($Net11) {
    if ($GlobalClientOptOut) {
      if ($page.Content.Contains('blazor-client-validation-data')) { throw 'Global opt-out emitted client rules.' }
    }
    else { Assert-Contains $page.Content 'blazor-client-validation-data' }
  }
  $null = Post-Form "/$mode" $valid 'Accepted'
  foreach ($field in @('Model.Lead', 'Model.Contact.Email', 'Model.Divers[1].Name')) {
    $invalid = $valid.Clone(); $invalid[$field] = ''
    $html = Post-Form "/$mode" $invalid 'Rejected'
    $expected = switch ($field) {
      'Model.Lead' { 'Enter a survey lead.' }
      'Model.Contact.Email' { 'Enter a contact address.' }
      default { 'Enter a diver name.' }
    }
    Assert-Contains $html $expected
  }
}
# The skipped snapshot remains invalid even in an accepted post. Its Required must not run.
$withSkipped = $valid.Clone(); $withSkipped['Model.Reference.ImportedCode'] = ''
$null = Post-Form '/enhanced' $withSkipped 'Accepted'
if ($Net11) {
  $optedOut = Invoke-WebRequest ([uri]::new($BaseUri, '/optout'))
  if ($optedOut.Content.Contains('blazor-client-validation-data')) { throw 'Per-form opt-out emitted client rules.' }
  $invalid = $valid.Clone(); $invalid['Model.Contact.Email'] = ''
  $null = Post-Form '/optout' $invalid 'Rejected'
}
$null = Post-Form '/diagnostics/undiscovered' @{ 'Model.Lead' = 'Reef team'; 'Model.Contact.Email' = '' } 'Top-level only: invalid child was missed'
$localized = [Net.WebUtility]::HtmlDecode((Invoke-WebRequest ([uri]::new($BaseUri, '/enhanced?culture=fr-FR&ui-culture=fr-FR'))).Content)
Assert-Contains $localized '<p id="caption">Adresse de contact</p>'
Assert-Contains $localized '<p id="fallback">ImportedCode</p>'
$report = @{ recordedAt = [DateTimeOffset]::UtcNow.ToString('O'); baseUri = "$BaseUri"; net11 = [bool]$Net11; globalClientOptOut = [bool]$GlobalClientOptOut; posts = $results; localizedCaption = 'Adresse de contact' }
if ($OutputPath) { $report | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $OutputPath }
Write-Host "PASS: $($results.Count) actual HTTP form posts; root/nested/collection validation, SkipValidation, missing-metadata boundary, localization and declared client opt-outs."
