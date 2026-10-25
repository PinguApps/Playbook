# Source/reference inspection only. No workload install, assembly execution or native build.
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$manifestPath = Join-Path $PSScriptRoot '../docs/planning/audits/mobile-api-gaps.sources.json'
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$records = @($manifest.sources | ForEach-Object -Parallel {
    $ErrorActionPreference = 'Stop'
    $source = $_
    $client = [System.Net.Http.HttpClient]::new()
    try {
        $bytes = $client.GetByteArrayAsync($source.url).GetAwaiter().GetResult()
        $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
        if ($hash -cne $source.sha256) { throw "Source hash mismatch: $($source.id)" }
        [pscustomobject]@{ id = $source.id; text = [Text.Encoding]::UTF8.GetString($bytes) }
    }
    finally { $client.Dispose() }
} -ThrottleLimit 8)
if ($records.Count -ne $manifest.sources.Count) { throw 'Incomplete source inspection' }

$texts = @{}
foreach ($record in $records) {
    if ($texts.ContainsKey($record.id)) { throw "Duplicate source ID: $($record.id)" }
    $texts[$record.id] = $record.text
}
foreach ($check in $manifest.checks) {
    if (!$texts.ContainsKey($check.source)) { throw "Missing check source: $($check.source)" }
    $found = $texts[$check.source].Contains($check.text, [StringComparison]::Ordinal)
    if ($found -ne $check.present) { throw "Source assertion failed: $($check.id)" }
}

$client = [System.Net.Http.HttpClient]::new()
try {
    foreach ($package in $manifest.packages) {
        $bytes = $client.GetByteArrayAsync($package.url).GetAwaiter().GetResult()
        $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
        if ($hash -cne $package.sha256) { throw "Package hash mismatch: $($package.platform)" }
        $archiveStream = [IO.MemoryStream]::new($bytes, $false)
        $zip = [IO.Compression.ZipArchive]::new($archiveStream, [IO.Compression.ZipArchiveMode]::Read)
        try {
            $entry = $zip.GetEntry($package.asset)
            if ($null -eq $entry) { throw "Missing reference: $($package.asset)" }
            $stream = $entry.Open()
            $reference = [IO.MemoryStream]::new()
            try { $stream.CopyTo($reference) }
            finally { $stream.Dispose() }
            $reference.Position = 0
            $pe = [Reflection.PortableExecutable.PEReader]::new($reference)
            try {
                $reader = [Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
                $fields = @()
                $quotaValue = $null
                foreach ($handle in $reader.TypeDefinitions) {
                    $type = $reader.GetTypeDefinition($handle)
                    if ($reader.GetString($type.Namespace) -cne 'HomeKit' -or
                        $reader.GetString($type.Name) -cne 'HMError') { continue }
                    foreach ($fieldHandle in $type.GetFields()) {
                        $field = $reader.GetFieldDefinition($fieldHandle)
                        $name = $reader.GetString($field.Name)
                        $fields += $name
                        if ($name -ceq 'QuotaExceeded') {
                            $constant = $reader.GetConstant($field.GetDefaultValue())
                            $blob = $reader.GetBlobReader($constant.Value)
                            $quotaValue = $blob.ReadInt64()
                        }
                    }
                }
                if ($fields.Count -ne $package.hmErrorFields -or $quotaValue -ne $package.quotaValue) {
                    throw "HMError reference mismatch: $($package.platform)"
                }
                Write-Output "$($package.platform): HMError fields=$($fields.Count); QuotaExceeded=$quotaValue"
            }
            finally { $pe.Dispose(); $reference.Dispose() }
        }
        finally { $zip.Dispose(); $archiveStream.Dispose() }
    }
}
finally { $client.Dispose() }

Write-Output "PASS: $($records.Count) pinned sources, $($manifest.checks.Count) source assertions, four Apple reference packs. No native execution."
