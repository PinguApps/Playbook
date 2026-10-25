[CmdletBinding()]
param(
  [string]$OutputPath = (Join-Path $PSScriptRoot '../artifacts/windowsdesktop.replayed.json'),
  [string]$CompareWith,
  [string]$SourceManifest = (Join-Path $PSScriptRoot '../docs/planning/audits/windowsdesktop.sources.json')
)

# Read package metadata only. No inspected assembly, analyzer or native UI is run.
$ErrorActionPreference = 'Stop'
$output = [IO.Path]::GetFullPath($OutputPath)
if ($CompareWith -and $output -eq [IO.Path]::GetFullPath($CompareWith)) { throw 'Do not overwrite the comparison baseline.' }
$scratch = [IO.Directory]::CreateTempSubdirectory('playbook-desktop-').FullName
function Get-Artifact([string]$Url, [int]$Status = 200) {
  $path = Join-Path $scratch ([guid]::NewGuid().ToString('N'))
  $response = Invoke-WebRequest -Uri $Url -OutFile $path -PassThru -SkipHttpErrorCheck
  if ([int]$response.StatusCode -ne $Status) { throw "Expected HTTP $Status from $Url; received $($response.StatusCode)." }
  [pscustomobject]@{ path = $path; sha256 = (Get-FileHash -LiteralPath $path).Hash.ToLowerInvariant() }
}
function Get-Diff([string[]]$Old, [string[]]$New) {
  $diff = [PlaybookDesktopMetadata]::Difference($Old, $New)
  [ordered]@{ added = $diff.Added; removed = $diff.Removed }
}

if (-not ('PlaybookDesktopMetadata' -as [type])) {
  Add-Type -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.RegularExpressions;

public sealed class PlaybookDesktopDiff {
    public string[] Added { get; set; }
    public string[] Removed { get; set; }
}
public sealed class PlaybookDesktopMetadata : ISignatureTypeProvider<string, object>
{
    private MetadataReader reader;
    public static PlaybookDesktopDiff Difference(string[] old, string[] current) => new PlaybookDesktopDiff {
        Added = current.Except(old, StringComparer.Ordinal).OrderBy(s => s, StringComparer.Ordinal).ToArray(),
        Removed = old.Except(current, StringComparer.Ordinal).OrderBy(s => s, StringComparer.Ordinal).ToArray()
    };
    public string GetArrayType(string t, ArrayShape s) => t + "[" + new string(',', s.Rank - 1) + "]";
    public string GetByReferenceType(string t) => t + "&";
    public string GetFunctionPointerType(MethodSignature<string> s) => "fnptr " + Signature(s);
    public string GetGenericInstantiation(string t, ImmutableArray<string> a) => t + "<" + string.Join(",", a) + ">";
    public string GetGenericMethodParameter(object c, int i) => "!!" + i;
    public string GetGenericTypeParameter(object c, int i) => "!" + i;
    public string GetModifiedType(string m, string t, bool required) => t + (required ? " modreq(" : " modopt(") + m + ")";
    public string GetPinnedType(string t) => t + " pinned";
    public string GetPointerType(string t) => t + "*";
    public string GetPrimitiveType(PrimitiveTypeCode c) => c.ToString();
    public string GetSZArrayType(string t) => t + "[]";
    public string GetTypeFromDefinition(MetadataReader r, TypeDefinitionHandle h, byte k) {
        var t = r.GetTypeDefinition(h);
        return t.IsNested ? GetTypeFromDefinition(r, t.GetDeclaringType(), k) + "+" + r.GetString(t.Name)
            : (r.GetString(t.Namespace) + "." + r.GetString(t.Name)).TrimStart('.');
    }
    public string GetTypeFromReference(MetadataReader r, TypeReferenceHandle h, byte k) {
        var t = r.GetTypeReference(h);
        return t.ResolutionScope.Kind == HandleKind.TypeReference
            ? GetTypeFromReference(r, (TypeReferenceHandle)t.ResolutionScope, k) + "+" + r.GetString(t.Name)
            : (r.GetString(t.Namespace) + "." + r.GetString(t.Name)).TrimStart('.');
    }
    public string GetTypeFromSpecification(MetadataReader r, object c, TypeSpecificationHandle h, byte k) => r.GetTypeSpecification(h).DecodeSignature(this, c);
    private string Name(EntityHandle h) => h.IsNil ? "" : h.Kind == HandleKind.TypeDefinition ? GetTypeFromDefinition(reader, (TypeDefinitionHandle)h, 0)
        : h.Kind == HandleKind.TypeReference ? GetTypeFromReference(reader, (TypeReferenceHandle)h, 0)
        : h.Kind == HandleKind.TypeSpecification ? GetTypeFromSpecification(reader, null, (TypeSpecificationHandle)h, 0) : h.Kind.ToString();
    private static string Signature(MethodSignature<string> s) => "(" + string.Join(",", s.ParameterTypes) + ")->" + s.ReturnType + " generic=" + s.GenericParameterCount + " required=" + s.RequiredParameterCount + " calling=" + s.Header.CallingConvention;
    private bool Visible(TypeDefinitionHandle h) {
        var t = reader.GetTypeDefinition(h);
        var v = t.Attributes & TypeAttributes.VisibilityMask;
        return v == TypeAttributes.Public || ((v == TypeAttributes.NestedPublic || v == TypeAttributes.NestedFamily || v == TypeAttributes.NestedFamORAssem) && Visible(t.GetDeclaringType()));
    }
    private static bool Visible(MethodAttributes a) {
        var v = a & MethodAttributes.MemberAccessMask;
        return v == MethodAttributes.Public || v == MethodAttributes.Family || v == MethodAttributes.FamORAssem;
    }
    private bool VisibleExport(ExportedTypeHandle h) {
        var t = reader.GetExportedType(h);
        var v = t.Attributes & TypeAttributes.VisibilityMask;
        return v == TypeAttributes.Public || ((v == TypeAttributes.NestedPublic || v == TypeAttributes.NestedFamily || v == TypeAttributes.NestedFamORAssem)
            && t.Implementation.Kind == HandleKind.ExportedType && VisibleExport((ExportedTypeHandle)t.Implementation));
    }
    private string Constant(ConstantHandle h) {
        if (h.IsNil) return "";
        var c = reader.GetConstant(h);
        return c.TypeCode + ":" + Convert.ToHexString(reader.GetBlobBytes(c.Value));
    }
    private string Attributes(CustomAttributeHandleCollection handles) => string.Join(" ", handles.Select(h => {
        var a = reader.GetCustomAttribute(h);
        var type = a.Constructor.Kind == HandleKind.MemberReference ? Name(reader.GetMemberReference((MemberReferenceHandle)a.Constructor).Parent)
            : Name(reader.GetMethodDefinition((MethodDefinitionHandle)a.Constructor).GetDeclaringType());
        // Preserve nullable, obsoletion, opt-in, default-value and other declaration metadata.
        // Blobs are values here (not signature tokens); serialize their exact bytes.
        return type + ":" + Convert.ToHexString(reader.GetBlobBytes(a.Value));
    }).OrderBy(s => s, StringComparer.Ordinal));
    public static string[] Surface(byte[] bytes, string prefix) {
        using var stream = new MemoryStream(bytes);
        using var pe = new PEReader(stream);
        var p = new PlaybookDesktopMetadata { reader = pe.GetMetadataReader() };
        var r = p.reader;
        var result = new List<string>();
        foreach (var h in r.TypeDefinitions) {
            if (!p.Visible(h)) continue;
            var t = r.GetTypeDefinition(h);
            var name = p.Name(h);
            result.Add("type " + name + " [" + t.Attributes + "] base=" + p.Name(t.BaseType) + " interfaces=" + string.Join(",", t.GetInterfaceImplementations().Select(i => p.Name(r.GetInterfaceImplementation(i).Interface)).OrderBy(s => s, StringComparer.Ordinal)) + " " + p.Attributes(t.GetCustomAttributes()));
            foreach (var gh in t.GetGenericParameters()) {
                var g = r.GetGenericParameter(gh);
                result.Add("generic " + name + " !" + g.Index + " [" + g.Attributes + "] " + string.Join(",", g.GetConstraints().Select(c => p.Name(r.GetGenericParameterConstraint(c).Type))) + " " + p.Attributes(g.GetCustomAttributes()));
            }
            foreach (var mh in t.GetMethods()) {
                var m = r.GetMethodDefinition(mh);
                if (!Visible(m.Attributes)) continue;
                var method = "method " + name + "::" + r.GetString(m.Name) + Signature(m.DecodeSignature(p, null));
                result.Add(method + " [" + m.Attributes + "] " + p.Attributes(m.GetCustomAttributes()));
                foreach (var ph in m.GetParameters()) {
                    var a = r.GetParameter(ph);
                    result.Add(method + " parameter=" + a.SequenceNumber + " " + r.GetString(a.Name) + " [" + a.Attributes + "] default=" + p.Constant(a.GetDefaultValue()) + " " + p.Attributes(a.GetCustomAttributes()));
                }
                foreach (var gh in m.GetGenericParameters()) {
                    var g = r.GetGenericParameter(gh);
                    result.Add(method + " generic=" + g.Index + " [" + g.Attributes + "] " + string.Join(",", g.GetConstraints().Select(c => p.Name(r.GetGenericParameterConstraint(c).Type))) + " " + p.Attributes(g.GetCustomAttributes()));
                }
            }
            foreach (var fh in t.GetFields()) {
                var f = r.GetFieldDefinition(fh);
                var v = f.Attributes & FieldAttributes.FieldAccessMask;
                if (v == FieldAttributes.Public || v == FieldAttributes.Family || v == FieldAttributes.FamORAssem)
                    result.Add("field " + name + "::" + r.GetString(f.Name) + " " + f.DecodeSignature(p, null) + " [" + f.Attributes + "] value=" + p.Constant(f.GetDefaultValue()) + " " + p.Attributes(f.GetCustomAttributes()));
            }
            foreach (var ph in t.GetProperties()) {
                var a = r.GetPropertyDefinition(ph);
                var access = a.GetAccessors();
                if ((!access.Getter.IsNil && Visible(r.GetMethodDefinition(access.Getter).Attributes)) || (!access.Setter.IsNil && Visible(r.GetMethodDefinition(access.Setter).Attributes)))
                    result.Add("property " + name + "::" + r.GetString(a.Name) + Signature(a.DecodeSignature(p, null)) + " [" + a.Attributes + "] default=" + p.Constant(a.GetDefaultValue()) + " " + p.Attributes(a.GetCustomAttributes()));
            }
            foreach (var eh in t.GetEvents()) {
                var e = r.GetEventDefinition(eh);
                var a = e.GetAccessors();
                if ((!a.Adder.IsNil && Visible(r.GetMethodDefinition(a.Adder).Attributes)) || (!a.Remover.IsNil && Visible(r.GetMethodDefinition(a.Remover).Attributes)))
                    result.Add("event " + name + "::" + r.GetString(e.Name) + " " + p.Name(e.Type) + " [" + e.Attributes + "] " + p.Attributes(e.GetCustomAttributes()));
            }
        }
        foreach (var h in r.ExportedTypes) {
            if (!p.VisibleExport(h)) continue;
            var t = r.GetExportedType(h);
            var destination = t.Implementation.Kind == HandleKind.AssemblyReference ? r.GetString(r.GetAssemblyReference((AssemblyReferenceHandle)t.Implementation).Name) : t.Implementation.Kind.ToString();
            result.Add("export " + r.GetString(t.Namespace) + "." + r.GetString(t.Name) + " [" + t.Attributes + "] -> " + destination);
        }
        return result.Select(s => prefix + s).OrderBy(s => s, StringComparer.Ordinal).ToArray();
    }
    public static string[] DiagnosticStrings(byte[] bytes) {
        using var stream = new MemoryStream(bytes);
        using var pe = new PEReader(stream);
        var r = pe.GetMetadataReader();
        var content = pe.GetMetadata().GetContent();
        var start = r.GetHeapMetadataOffset(HeapIndex.UserString);
        var end = start + r.GetHeapSize(HeapIndex.UserString);
        var ids = new SortedSet<string>(StringComparer.Ordinal);
        for (int i = start + 1; i < end;) {
            int size = content[i++];
            if ((size & 0x80) != 0) {
                if ((size & 0xC0) == 0x80) size = ((size & 0x3F) << 8) | content[i++];
                else { size = ((size & 0x1F) << 24) | (content[i++] << 16) | (content[i++] << 8) | content[i++]; }
            }
            if (size == 0) continue;
            if (i + size > end) throw new BadImageFormatException("Invalid user string heap.");
            var text = Encoding.Unicode.GetString(content.AsSpan(i, size - 1));
            foreach (Match m in Regex.Matches(text, @"\bWFO\d{4}\b")) ids.Add(m.Value);
            i += size;
        }
        return ids.ToArray();
    }
}
'@
}

try {
  $manifest = Get-Content -LiteralPath $SourceManifest -Raw | ConvertFrom-Json
  $sources = @($manifest.sources | ForEach-Object {
    $artifact = Get-Artifact $_.url
    if ($artifact.sha256 -cne $_.sha256) { throw "Source hash changed: $($_.id)" }
    [ordered]@{ id = $_.id; url = $_.url; sha256 = $artifact.sha256 }
  })
  $history = @($manifest.missingNotes | ForEach-Object {
    $null = Get-Artifact $_.url 404
    $response = Invoke-RestMethod $_.historyUrl
    if (@($response).Count -ne 0) { throw "Canonical note history changed: $($_.url)" }
    [ordered]@{ url = $_.url; status = 404; historyUrl = $_.historyUrl; historyCommits = 0 }
  })
  $snapshots = @{}
  $packages = @($manifest.packages | ForEach-Object {
    $package = $_
    Write-Host "Inspecting $($package.id) ($($package.version))"
    $artifact = Get-Artifact $package.url
    $zip = [IO.Compression.ZipFile]::OpenRead($artifact.path)
    try {
      $nuspec = $zip.Entries | Where-Object FullName -like '*.nuspec'
      $reader = [IO.StreamReader]::new($nuspec.Open())
      try { [xml]$packageSpec = $reader.ReadToEnd() } finally { $reader.Dispose() }
      if ([string]$packageSpec.package.metadata.version -cne $package.version) { throw 'Package version mismatch.' }
      $surface = [Collections.Generic.List[string]]::new()
      $assets = @($zip.Entries | Where-Object { $_.FullName -match '^ref/net(10|11)\.0/[^/]+\.dll$|^analyzers/dotnet/(cs/|vb/)?[^/]+\.dll$' } | Sort-Object FullName | ForEach-Object {
        $entry = $_
        $stream = [IO.MemoryStream]::new()
        $inputStream = $entry.Open()
        try { $inputStream.CopyTo($stream) } finally { $inputStream.Dispose() }
        $bytes = $stream.ToArray(); $stream.Dispose()
        $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
        if ($entry.FullName.StartsWith('ref/')) {
          $api = [PlaybookDesktopMetadata]::Surface($bytes, "$([IO.Path]::GetFileName($entry.FullName)): ")
          $surface.AddRange($api)
          [ordered]@{ asset = $entry.FullName; sha256 = $hash; declarations = $api.Count }
        } else {
          [ordered]@{ asset = $entry.FullName; sha256 = $hash; diagnosticStrings = @([PlaybookDesktopMetadata]::DiagnosticStrings($bytes)) }
        }
      })
      if (-not $surface.Count) { throw 'Reference assets missing.' }
      $snapshots[$package.id] = $surface.ToArray()
      [ordered]@{ id = $package.id; version = $package.version; url = $package.url; sha256 = $artifact.sha256; repository = [string]$packageSpec.package.metadata.repository.url; commit = [string]$packageSpec.package.metadata.repository.commit; assets = $assets }
    } finally { $zip.Dispose() }
  })
  # The Visual Studio out-of-process extension SDK has its own release line.
  # Inventory its real assets without treating it as a .NET 11 framework pack.
  $sdk = Get-Artifact $manifest.designerSdk.url
  $zip = [IO.Compression.ZipFile]::OpenRead($sdk.path)
  try {
    $reader = [IO.StreamReader]::new(($zip.Entries | Where-Object FullName -like '*.nuspec').Open())
    try { [xml]$designerSpec = $reader.ReadToEnd() } finally { $reader.Dispose() }
    if ([string]$designerSpec.package.metadata.version -cne $manifest.designerSdk.version) { throw 'Designer SDK version mismatch.' }
    $sdkAssets = @($zip.Entries | Where-Object { $_.FullName -match '\.(dll|props|targets)$' } | Sort-Object FullName | ForEach-Object {
      $stream = [IO.MemoryStream]::new(); $inputStream = $_.Open()
      try { $inputStream.CopyTo($stream) } finally { $inputStream.Dispose() }
      try { $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream.ToArray())).ToLowerInvariant() } finally { $stream.Dispose() }
      [ordered]@{ asset = $_.FullName; sha256 = $hash }
    })
    $designerSdk = [ordered]@{
      url = $manifest.designerSdk.url; version = [string]$designerSpec.package.metadata.version; sha256 = $sdk.sha256
      repository = [string]$designerSpec.package.metadata.repository.url; commit = [string]$designerSpec.package.metadata.repository.commit
      dependencyGroups = @($designerSpec.package.metadata.dependencies.group | ForEach-Object {
        [ordered]@{ target = [string]$_.targetFramework; dependencies = @($_.dependency | ForEach-Object { "$($_.id)/$($_.version)" }) }
      }); assets = $sdkAssets
    }
  } finally { $zip.Dispose() }
  $transitions = @()
  for ($i = 1; $i -lt $packages.Count; $i++) {
    $old = $packages[$i - 1].id; $new = $packages[$i].id
    $diff = Get-Diff $snapshots[$old] $snapshots[$new]
    $transitions += [ordered]@{ from = $old; to = $new; added = $diff.added.Count; removed = $diff.removed.Count; changes = $diff }
  }
  foreach ($transition in $transitions | Where-Object { $_.to -in @('preview2', 'preview3') }) {
    if ($transition.added -ne 0 -or $transition.removed -ne 0) { throw 'Preview 2/3 no-declaration-change disposition no longer holds.' }
  }
  $oldRules = @($packages[0].assets.diagnosticStrings | Sort-Object -Unique)
  $newRules = @($packages[-1].assets.diagnosticStrings | Sort-Object -Unique)
  $addedRules = @($newRules | Where-Object { $_ -notin $oldRules })
  $expectedRules = @('WFO2000') + @(3000..3012 | ForEach-Object { "WFO$_" })
  if (@(Compare-Object $expectedRules $addedRules).Count) { throw 'RC2 diagnostic ownership changed.' }
  if (-not @($snapshots['rc2'] | Where-Object { $_ -match '^System.Windows.Forms.dll: property System.Windows.Forms.Application::IsFormRevealDeferred\(' }).Count) {
    throw 'Resolved reveal property positive control missing.'
  }
  $report = [ordered]@{
    scope = 'PIN-693; public/protected reference declarations, declaration attributes and analyzer string presence only; no sample/analyzer/native execution or GA proof'
    sources = $sources; missingNotes = $history; packages = $packages; designerSdk = $designerSdk; transitions = $transitions
    net10ToRc2 = Get-Diff $snapshots['net10'] $snapshots['rc2']
  }
  $json = $report | ConvertTo-Json -Depth 20
  if ($CompareWith) {
    $expected = Get-Content -LiteralPath $CompareWith -Raw | ConvertFrom-Json | ConvertTo-Json -Depth 20
    if ($expected -cne $json) { throw 'WindowsDesktop evidence changed; inspect the sources, assets and dispositions.' }
  }
  [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($output)) | Out-Null
  $json | Set-Content -LiteralPath $output -Encoding utf8
  $transitions | ForEach-Object { [pscustomobject]$_ } | Select-Object from, to, added, removed | Format-Table -AutoSize
  Write-Host "PASS: $($sources.Count) immutable sources; two missing notes reconciled through bounded canonical history; $($packages.Count) real reference/analyzer packages."
} finally {
  # Only flat files created in this invocation's unique temporary directory.
  Get-ChildItem -LiteralPath $scratch -File | Remove-Item -Force
  Remove-Item -LiteralPath $scratch
}
