# Downloads and inspects official artifacts only. No workload installation or assembly execution.
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$manifest = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../docs/planning/audits/mobile-baselines.sources.json') -Raw | ConvertFrom-Json -AsHashtable
$client = [Net.Http.HttpClient]::new()
$client.DefaultRequestHeaders.UserAgent.ParseAdd('Playbook-MobileBaselineAudit')

function Assert-Hash([byte[]]$Bytes, $Record) {
    $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($Bytes)).ToLowerInvariant()
    if ($hash -cne $Record.sha256) { throw "Hash mismatch: $($Record.id)" }
}

function Read-Entry($Zip, [string]$Name) {
    $entry = $Zip.GetEntry($Name)
    if ($null -eq $entry) { throw "Missing package entry: $Name" }
    $reader = [IO.StreamReader]::new($entry.Open())
    try { $reader.ReadToEnd() } finally { $reader.Dispose() }
}

try {
    $texts = @{}
    foreach ($source in $manifest.sources) {
        if ($texts.ContainsKey($source.id)) { throw "Duplicate source: $($source.id)" }
        $bytes = $client.GetByteArrayAsync($source.url).GetAwaiter().GetResult()
        if ($source.ContainsKey('jsonField')) {
            $json = [Text.Encoding]::UTF8.GetString($bytes) | ConvertFrom-Json -AsHashtable
            $bytes = [Text.Encoding]::UTF8.GetBytes($json[$source.jsonField])
        }
        # Release bodies and the wiki are mutable: a changed snapshot requires review, never silent acceptance.
        Assert-Hash $bytes $source
        $texts[$source.id] = [Text.Encoding]::UTF8.GetString($bytes)
    }
    foreach ($check in $manifest.checks) {
        if (!$texts.ContainsKey($check.source)) { throw "Unknown source: $($check.source)" }
        if ($texts[$check.source].Contains($check.text, [StringComparison]::Ordinal) -ne $check.present) {
            throw "Source assertion failed: $($check.id)"
        }
    }

    $sets = [Collections.Generic.List[object]]::new()
    $platformManifests = @{}
    foreach ($package in $manifest.packages) {
        $bytes = $client.GetByteArrayAsync($package.url).GetAwaiter().GetResult()
        Assert-Hash $bytes $package
        $stream = [IO.MemoryStream]::new($bytes, $false)
        $zip = [IO.Compression.ZipArchive]::new($stream, [IO.Compression.ZipArchiveMode]::Read)
        try {
            $nuspecEntries = @($zip.Entries | Where-Object FullName -Like '*.nuspec')
            if ($nuspecEntries.Count -ne 1) { throw 'Expected one package identity' }
            [xml]$nuspec = Read-Entry $zip $nuspecEntries[0].FullName
            $metadata = $nuspec.package.metadata
            if ($metadata.id -cne $package.id -or $metadata.version -cne $package.version -or
                $metadata.repository.commit -cne $package.repositoryCommit) {
                throw "Package identity/provenance mismatch: $($package.id)"
            }
            if ($package.kind -eq 'set') {
                $map = Read-Entry $zip 'data/microsoft.net.workloads.workloadset.json' | ConvertFrom-Json -AsHashtable
                if ($map.Count -ne $package.manifests.Count) { throw 'Workload set manifest count changed' }
                foreach ($key in $package.manifests.Keys) {
                    if ($map[$key] -cne $package.manifests[$key]) { throw "Set composition changed: $key" }
                }
                $sets.Add($map)
                Write-Output "Set $($package.version): $($map.Count) exact manifest entries; commit $($package.repositoryCommit)"
            }
            elseif ($package.kind -eq 'manifest') {
                $data = Read-Entry $zip 'data/WorkloadManifest.json' | ConvertFrom-Json -AsHashtable
                if ($data.version -cne $package.version) { throw "Manifest version changed: $($package.id)" }
                foreach ($key in $package.packs.Keys) {
                    if ($data.packs[$key].version -cne $package.packs[$key]) { throw "Pack selection changed: $key" }
                }
                $platformManifests[$package.id.Replace('.Manifest-11.0.100-rc.2','')] = $package.version
                Write-Output "$($package.id): $($package.version); $($package.packs.Count) selected pack versions checked"
            }
            elseif ($package.kind -eq 'ai') {
                $assets = @($zip.Entries.FullName | Where-Object { $_ -like 'lib/*.dll' } | Sort-Object)
                if (($assets -join '|') -cne (($package.assets | Sort-Object) -join '|')) { throw 'AI target assets changed' }
                $entryStream = $zip.GetEntry($package.experimentalAsset).Open()
                $assembly = [IO.MemoryStream]::new()
                try { $entryStream.CopyTo($assembly) } finally { $entryStream.Dispose() }
                $assembly.Position = 0
                $pe = [Reflection.PortableExecutable.PEReader]::new($assembly)
                try {
                    $reader = [Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
                    $types = @($reader.TypeDefinitions | ForEach-Object { $reader.GetString($reader.GetTypeDefinition($_).Name) })
                    foreach ($name in $package.requiredTypes) {
                        if ($name -cnotin $types) { throw "Missing AI type: $name" }
                    }
                    $experimental = $false
                    foreach ($handle in $reader.GetAssemblyDefinition().GetCustomAttributes()) {
                        $attribute = $reader.GetCustomAttribute($handle)
                        if ($attribute.Constructor.Kind -ne 'MemberReference') { continue }
                        $constructor = $reader.GetMemberReference([Reflection.Metadata.MemberReferenceHandle]$attribute.Constructor)
                        if ($constructor.Parent.Kind -ne 'TypeReference') { continue }
                        $type = $reader.GetTypeReference([Reflection.Metadata.TypeReferenceHandle]$constructor.Parent)
                        if ($reader.GetString($type.Namespace) -cne 'System.Diagnostics.CodeAnalysis' -or
                            $reader.GetString($type.Name) -cne 'ExperimentalAttribute') { continue }
                        $blob = $reader.GetBlobReader($attribute.Value)
                        if ($blob.ReadUInt16() -ne 1) { throw 'Invalid attribute prolog' }
                        if ($blob.ReadSerializedString() -ceq $package.diagnostic) { $experimental = $true }
                    }
                    if (!$experimental) { throw 'AI experimental assembly diagnostic missing' }
                }
                finally { $pe.Dispose(); $assembly.Dispose() }
                Write-Output "AI $($package.version): $($assets.Count) net10.0 assets; assembly experimental $($package.diagnostic)"
            }
            else { throw "Unknown package kind: $($package.kind)" }
        }
        finally { $zip.Dispose(); $stream.Dispose() }
    }
    if ($sets.Count -ne 2 -or $platformManifests.Count -ne 6) { throw 'Incomplete workload composition audit' }
    foreach ($key in $sets[0].Keys) {
        if ($sets[0][$key] -cne $sets[1][$key]) { throw "Rebuilt set differs: $key" }
    }
    foreach ($key in $platformManifests.Keys) {
        foreach ($set in $sets) {
            if ($set[$key] -cne "$($platformManifests[$key])/11.0.100-rc.2") { throw "Manifest/set mismatch: $key" }
        }
    }
    Write-Output "PASS: $($texts.Count) pinned source snapshots, $($manifest.checks.Count) source assertions, $($manifest.packages.Count) package hashes/provenance; two identical 18-entry sets. No installation, compilation or native execution."
}
finally { $client.Dispose() }
