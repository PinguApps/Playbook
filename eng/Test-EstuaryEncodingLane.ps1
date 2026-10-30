param([string]$Dotnet,[string]$Directory,[string]$Reader,[string]$Output,[int]$ConsoleCodePage)
$ErrorActionPreference='Stop'
if(-not $env:ESTUARY_PRIVATE_CONSOLE_OWNER){throw 'Run through the private-console fixture'}
Add-Type -TypeDefinition @'
using System.Runtime.InteropServices;
[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
public static class EstuaryConsole {
  [DllImport("kernel32")] public static extern bool SetConsoleOutputCP(uint value);
  [DllImport("kernel32")] public static extern bool SetConsoleCP(uint value);
  [DllImport("kernel32")] public static extern uint GetConsoleOutputCP();
  [DllImport("kernel32")] public static extern uint GetConsoleProcessList([Out] uint[] list, uint count);
}
'@
$members=[uint[]]::new(10)
$count=[EstuaryConsole]::GetConsoleProcessList($members,10)
if($count -ne 2 -or $members -notcontains [uint]$env:ESTUARY_PRIVATE_CONSOLE_OWNER -or $members -notcontains [uint]$PID){throw 'Refuse a console not exclusively owned by this fixture'}
$results=@()
foreach($case in @(@{force='default';source=$false},@{force='false';source=$false},@{force='true';source=$false},@{force='default';source=$true})) {
  $force=$case.force
  # Both PowerShell and prior SDK calls can select UTF-8. Reset this private console
  # before each control, rather than accidentally testing the previous call's page.
  if(-not [EstuaryConsole]::SetConsoleCP($ConsoleCodePage) -or -not [EstuaryConsole]::SetConsoleOutputCP($ConsoleCodePage)){throw 'Private code-page reset failed'}
  $before=[EstuaryConsole]::GetConsoleOutputCP()
  # Capture once with a lossless byte-to-character mapping; select the actual decoder below.
  $r=& $Reader -Dotnet $Dotnet -ProjectDirectory $Directory -ForceUtf8 $force -ReaderCodePage 28591 -ProbeSourceOptOut:$case.source
  if($r.ExitCode -ne 0){throw "Build failed: $($r.Stderr)"}
  $hex=[Convert]::ToHexString($r.StdoutBytes)
  $page=if($hex.Contains('636166C3A9')){65001}elseif($hex.Contains('636166E9')){1252}else{throw 'Non-ASCII diagnostic bytes missing'}
  $text=[Text.Encoding]::GetEncoding($page).GetString($r.StdoutBytes)
  if(-not $text.Contains('ESTUARY001: Estuary café: berth fermé')){throw 'Diagnostic decode mismatch'}
  $results+=@{force=$force;sourceOptOut=$case.source;codePage=$page;consoleBefore=$before;consoleAfter=[EstuaryConsole]::GetConsoleOutputCP();exit=$r.ExitCode;diagnostic='ESTUARY001: Estuary café: berth fermé';bytes=if($page -eq 65001){'636166c3a9'}else{'636166e9'}}
}
$failure=& $Reader -Dotnet $Dotnet -ProjectDirectory $Directory -ForceUtf8 true -Reject
if($failure.ExitCode -eq 0 -or -not $failure.Stdout.Contains('ESTUARY002: Estuary café: berth fermé')){throw 'Expected real failed build'}
$results+=@{force='true';exit=$failure.ExitCode;diagnostic='ESTUARY002: Estuary café: berth fermé';readerCodePage=65001}
$wrong=[Text.Encoding]::GetEncoding(1252).GetString($failure.StdoutBytes)
if($wrong.Contains('ESTUARY002: Estuary café: berth fermé')){throw 'Wrong-decoder control did not corrupt non-ASCII bytes'}
[IO.File]::WriteAllText($Output,($results|ConvertTo-Json -Depth 5))
Write-Host ($results|ConvertTo-Json -Compress)
