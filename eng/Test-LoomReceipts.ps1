[CmdletBinding()]
param([Parameter(Mandatory)][string]$Receipts)

# Inspect actual OTLP JSON written by the official collector's file exporter.
# This is not an exporter, listener, HTTP stub or browser execution substitute.
$ErrorActionPreference = 'Stop'
$rows = @(Get-Content -LiteralPath $Receipts | Where-Object { $_.Trim() } | ForEach-Object { $_ | ConvertFrom-Json -AsHashtable })
function Attribute($Attributes, [string]$Key) {
  ($Attributes | Where-Object key -CEQ $Key).value.stringValue
}
foreach ($version in @('10.0', '11.0')) {
  $spans = @(); $metrics = @()
  foreach ($row in $rows) {
    foreach ($resource in @($row.resourceSpans)) {
      if ((Attribute $resource.resource.attributes 'service.name') -cne 'LoomActivity' -or
          (Attribute $resource.resource.attributes 'service.version') -cne $version) { continue }
      foreach ($scope in $resource.scopeSpans) {
        foreach ($span in $scope.spans) { $spans += @{ scope = $scope.scope.name; span = $span } }
      }
    }
    foreach ($resource in @($row.resourceMetrics)) {
      if ((Attribute $resource.resource.attributes 'service.name') -cne 'LoomActivity' -or
          (Attribute $resource.resource.attributes 'service.version') -cne $version) { continue }
      foreach ($scope in $resource.scopeMetrics) {
        foreach ($metric in $scope.metrics) { $metrics += @{ scope = $scope.scope.name; metric = $metric } }
      }
    }
  }
  $events = @($spans | Where-Object { $_.scope -ceq 'Microsoft.AspNetCore.Components' -and
    (Attribute $_.span.attributes 'aspnetcore.components.type') -ceq 'LoomActivity.Board' -and
    (Attribute $_.span.attributes 'code.function.name') -ceq 'ScheduleShuttle' -and
    (Attribute $_.span.attributes 'aspnetcore.components.attribute.name') -ceq 'onclick' })
  if (-not $events.Count) { throw "No actual framework click span for $version" }
  $correlated = @($events | Where-Object {
    $eventSpan = $_.span
    @($spans | Where-Object { $_.scope -ceq 'LoomActivity.Work' -and $_.span.name -ceq 'loom.schedule' -and
      $_.span.traceId -ceq $eventSpan.traceId -and $_.span.parentSpanId -ceq $eventSpan.spanId }).Count -gt 0 -and
    @($spans | Where-Object { $routeSpan = $_.span
      $_.scope -ceq 'Microsoft.AspNetCore.Components' -and
      (Attribute $routeSpan.attributes 'aspnetcore.components.route') -ceq '/' -and
      @($eventSpan.links | Where-Object { $_.traceId -ceq $routeSpan.traceId -and $_.spanId -ceq $routeSpan.spanId }).Count -gt 0
    }).Count -gt 0
  })
  if (-not $correlated.Count) { throw "No received event -> child trace parent and event -> route link correlation for $version" }
  if (-not @($spans | Where-Object { $_.scope -ceq 'Microsoft.AspNetCore.Components' -and
      (Attribute $_.span.attributes 'aspnetcore.components.route') -ceq '/dispatch' }).Count) { throw "No dispatch navigation span for $version" }
  foreach ($name in @('navigate', 'handle_event.duration', 'update_parameters.duration', 'render_diff.duration', 'render_diff.size')) {
    $scopeName = if ($name -in @('navigate', 'handle_event.duration')) { 'Microsoft.AspNetCore.Components' } else { 'Microsoft.AspNetCore.Components.Lifecycle' }
    $matching = @($metrics | Where-Object { $_.scope -ceq $scopeName -and $_.metric.name -ceq "aspnetcore.components.$name" })
    if (-not $matching.Count) { throw "Missing received $scopeName/$name for $version" }
    $points = @($matching | ForEach-Object { @($_.metric.histogram.dataPoints) + @($_.metric.sum.dataPoints) } | Where-Object { $_ })
    if (-not @($points | Where-Object { [long]$_.count -gt 0 -or [long]$_.asInt -gt 0 }).Count) { throw "No positive actual measurements: $version/$name" }
    if ($name -eq 'handle_event.duration' -and -not @($points | Where-Object {
      (Attribute $_.attributes 'aspnetcore.components.type') -ceq 'LoomActivity.Board' -and
      (Attribute $_.attributes 'code.function.name') -ceq 'ScheduleShuttle'
    }).Count) { throw "Missing component/method metric identity for $version" }
  }
  Write-Output "PASS $version`: received five component instruments, click/method identity, dispatch span, event child parent and route link correlation."
}
