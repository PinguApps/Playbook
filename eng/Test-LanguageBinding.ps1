[CmdletBinding()]
param([string]$Dotnet = 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$output = Join-Path $root ('artifacts/language-binding/' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $output -Force | Out-Null
$install = Split-Path $Dotnet -Parent
$compiler = Join-Path $install 'sdk/11.0.100/Roslyn/bincore/csc.dll'
$refs = @(Get-ChildItem (Join-Path $install 'packs/Microsoft.NETCore.App.Ref/11.0.0/ref/net11.0') -Filter '*.dll' | ForEach-Object { '-r:' + $_.FullName })
$common = @($compiler, '-nologo', '-nostdlib+', '-langversion:15', '-nullable:enable', '-warnaserror+') + $refs
function Compile([string]$Name, [string]$Source, [string]$Diagnostic = '', [string[]]$Extra = @()) {
    $file = Join-Path $output "$Name.cs"
    $binary = Join-Path $output "$Name.dll"
    $Source | Set-Content -LiteralPath $file
    $PSNativeCommandUseErrorActionPreference = $false
    $result = & $Dotnet @common '-target:library' "-out:$binary" @Extra $file 2>&1 | Out-String
    $code = $LASTEXITCODE
    $result | Set-Content (Join-Path $output "$Name.log")
    if (($Diagnostic -and ($code -eq 0 -or $result -notmatch $Diagnostic)) -or (-not $Diagnostic -and $code -ne 0)) { throw "$Name : $result" }
    Write-Host "PASS $Name $Diagnostic"
    return $binary
}
$owner = Compile owner @'
public closed class Decision;
public sealed class Accepted : Decision;
internal sealed class Hidden : Decision;
public class Open : Decision;
'@
$reference = @('-r:' + $owner)
$null = Compile direct 'public class External : Decision;' 'CS9382' $reference
$null = Compile indirect 'public class External : Open;' '' $reference
$null = Compile hidden 'public class P { public int M(Decision x) => x switch { Accepted => 1, Open => 2 }; }' 'CS8509' $reference
$null = Compile hiddenName 'public class P { public int M(Decision x) => x switch { Hidden => 1, _ => 0 }; }' 'CS0122' $reference
$null = Compile fallback 'public class P { public int M(Decision? x) => x switch { Accepted => 1, Open => 2, Decision => 3, null => 0 }; }' '' $reference
$basic = 'public closed class Shelf; public sealed class A : Shelf; public sealed class B : Shelf; '
$null = Compile exhaustive ($basic + 'public class P { public int M(Shelf? x) => x switch { A => 1, B => 2, null => 0 }; }')
$null = Compile subsumed ($basic + 'public class P { public int M(Shelf x) => x switch { A => 1, B => 2, Shelf => 3 }; }') 'CS8510'
$null = Compile nullable ($basic + 'public class P { public int M(Shelf? x) => x switch { A => 1, B => 2 }; }') 'CS8655'
$null = Compile statement ($basic + 'public class P { public void M(Shelf? x) { switch(x) { case A: break; } } }')
$generic = 'public closed class Shelf<T>; public sealed class One<T> : Shelf<T>; public sealed class Many<T> : Shelf<T[]>; '
$null = Compile unused 'public closed class Shelf<T>; public class Invalid<T> : Shelf<int>;' 'CS9383'
$null = Compile filtered ($generic + 'public class P { public int M(Shelf<string> x) => x switch { One<string> => 1 }; }')
$null = Compile unspeakable ($generic + 'public class P { public int M<T>(Shelf<T> x) => x switch { One<T> => 1 }; }') 'CS8509'
$null = Compile genericFallback ($generic + 'public class P { public int M<T>(Shelf<T> x) => x switch { One<T> => 1, Shelf<T> => 2 }; }')
$null = Compile constraints 'public closed class Shelf<T>; public sealed class One<T> : Shelf<T>; public sealed class Value<T> : Shelf<T> where T:struct; public class P { public int M<T>(Shelf<T> x) where T:class => x switch { One<T> => 1 }; }' 'CS8509'
$imports = 'using System; using System.Collections.Generic; '
$overloads = $imports + 'public class P { static void Store(List<string> x) {} static void Store(HashSet<string> x) {} public void M() { '
$null = Compile outer ($overloads + 'Store([with(StringComparer.OrdinalIgnoreCase), "bay"]); } }') 'CS0121'
$null = Compile target ($overloads + 'HashSet<string> labels = [with(comparer: StringComparer.OrdinalIgnoreCase), "bay"]; Store(labels); } }')
$infer = $imports + 'public class P { static void Store<T>(HashSet<T> x) {} public void M() { '
$null = Compile inference ($infer + 'Store([with(StringComparer.OrdinalIgnoreCase)]); } }') 'CS0411'
$null = Compile explicit ($infer + 'Store<string>([with(comparer: StringComparer.OrdinalIgnoreCase)]); } }')
$null = Compile defaultLiteral ($imports + 'public class P { public List<int> M() => [with(default)]; }') 'CS0121'
$null = Compile named ($imports + 'public class P { public List<int> M() => [with(capacity: default)]; }')
$null = Compile classic ($imports + 'public class P { public List<int> M() => new() { with(3) }; }') 'CS0103'
$null = Compile dictionary ($imports + 'public class P { public Dictionary<string,int> M() => ["bay": 1]; }') 'CS1003'
$trace = Join-Path $output 'Trace.cs'
@'
using System;
using System.Collections.Generic;
public static class Trace
{
    static readonly List<string> Calls = new();
    static int Mark(string name, int value) { Calls.Add(name); return value; }
    static IEnumerable<int> Fetch() { Calls.Add("fetch"); return Iterate(); }
    static IEnumerable<int> Iterate() { Calls.Add("enumerate"); yield return 2; }
    public static void Main()
    {
        List<int> values = [with(capacity: Mark("reserve", 8)), Mark("first", 1), .. Fetch(), Mark("last", 3)];
        // Check expression evaluation separately from iterator scheduling.
        var expressions = Calls.FindAll(x => x != "enumerate");
        if (string.Join(",", expressions) != "reserve,first,fetch,last" ||
            Calls.FindAll(x => x == "enumerate").Count != 1 ||
            string.Join(",", values) != "1,2,3") throw new Exception("Evaluation contract changed");
        Console.WriteLine("PASS evaluation: " + string.Join(",", Calls));
    }
}
'@ | Set-Content $trace
$binary = Join-Path $output 'Trace.dll'
& $Dotnet @common '-target:exe' "-out:$binary" $trace
if ($LASTEXITCODE) { throw 'Trace compilation failed' }
@{runtimeOptions=@{tfm='net11.0';framework=@{name='Microsoft.NETCore.App';version='11.0.0'};rollForward='Disable'}} |
    ConvertTo-Json -Depth 6 | Set-Content (Join-Path $output 'Trace.runtimeconfig.json')
& $Dotnet $binary
if ($LASTEXITCODE) { throw 'Trace execution failed' }
Write-Host "Compiler: $(& $Dotnet $compiler -version); SDK11.0.100 refs11.0.0; logs: $output"


