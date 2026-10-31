[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe',
  [string]$OutputDirectory = ([IO.Path]::GetTempPath()),
  [switch]$Launch
)

# Local capability probes, never recipe evidence or a native CI matrix.
$ErrorActionPreference = 'Stop'
if (-not $IsWindows) { throw 'Run on the existing Windows authoring machine.' }
$output = [IO.Path]::GetFullPath($OutputDirectory)
$work = Join-Path $output ('pbw-' + [guid]::NewGuid().ToString('N').Substring(0, 8))
[IO.Directory]::CreateDirectory($work) | Out-Null
$paths = @{}
foreach ($scope in @('Process', 'User', 'Machine')) {
  $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope)
}
$previous = @{}
foreach ($key in @('DOTNET_ADD_GLOBAL_TOOLS_TO_PATH', 'DOTNET_CLI_TELEMETRY_OPTOUT', 'DOTNET_CLI_HOME', 'DOTNET_GENERATE_ASPNET_CERTIFICATE', 'DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE', 'NUGET_PACKAGES')) {
  $previous[$key] = [Environment]::GetEnvironmentVariable($key)
}
function Write-Input([string]$Path, [string]$Value) {
  [IO.Directory]::CreateDirectory((Split-Path $Path)) | Out-Null
  [IO.File]::WriteAllText($Path, $Value.Replace("`r`n", "`n") + "`n")
}
function Invoke-Dotnet([string]$Executable, [string[]]$Arguments, [string]$Log) {
  $PSNativeCommandUseErrorActionPreference = $false
  $result = (& $Executable @Arguments 2>&1 | Out-String).Trim()
  $code = $LASTEXITCODE
  Write-Input $Log $result
  if ($code -ne 0) { throw "$Executable $($Arguments -join ' ') failed ($code). See $Log`n$result" }
  $result
}
$desktop = @'
using System;
using System.IO;
using System.Runtime.InteropServices;
internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
#if WPF
        var application = new System.Windows.Application();
        var window = new System.Windows.Window { Title = "Playbook WPF probe", Width = 320, Height = 120 };
        window.Content = new System.Windows.Controls.TextBlock { Text = "Willow Quay authoring probe" };
        window.ContentRendered += (_, _) => {
            File.WriteAllText(args[0], "ContentRendered / " + RuntimeInformation.FrameworkDescription);
            window.Close();
        };
        application.Run(window);
#else
        System.Windows.Forms.Application.EnableVisualStyles();
        System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
        using var window = new System.Windows.Forms.Form { Text = "Playbook WinForms probe", Width = 320, Height = 120 };
        window.Controls.Add(new System.Windows.Forms.Label { Text = "Willow Quay authoring probe", AutoSize = true });
        window.Shown += (_, _) => {
            File.WriteAllText(args[0], "Shown / " + RuntimeInformation.FrameworkDescription);
            window.BeginInvoke(window.Close);
        };
        System.Windows.Forms.Application.Run(window);
#endif
    }
}
'@
$maui = @'
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
namespace WindowsAuthoring;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp() => MauiApp.CreateBuilder().UseMauiApp<ProbeApp>().Build();
}
public sealed class ProbeApp : Application
{
    protected override Window CreateWindow(IActivationState? state)
    {
        var label = new Label { Text = "Willow Quay authoring probe" };
        var window = new Window(new ContentPage { Content = label }) { Title = "Playbook MAUI probe" };
        label.Loaded += (_, _) => {
            System.IO.File.WriteAllText(System.IO.Path.Combine(AppContext.BaseDirectory, "launch-result.txt"),
                "Label.Loaded / " + System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
            label.Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(1), () => CloseWindow(window));
        };
        return window;
    }
}
'@
$routes = [Collections.Generic.List[object]]::new()
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
  $env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
  $env:DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE = 'true'
  $env:DOTNET_CLI_HOME = Join-Path $work 'h'
  $env:NUGET_PACKAGES = Join-Path $work 'p'
  # Explicit boundaries stop generated consumers inheriting the tool's build/packages.
  Write-Input (Join-Path $work 'Directory.Build.props') '<Project />'
  Write-Input (Join-Path $work 'Directory.Build.targets') '<Project />'
  Write-Input (Join-Path $work 'Directory.Packages.props') '<Project />'
  foreach ($baseline in @(
    @{ target = 'net10.0'; sdk = '10.0.402'; dotnet = $Net10Dotnet; maui = '10.0.110' },
    @{ target = 'net11.0'; sdk = '11.0.100-rc.2.26504.105'; dotnet = $Net11Dotnet; maui = '11.0.0-preview.6.26360.8' }
  )) {
    foreach ($framework in @('Wpf', 'WinForms', 'Maui')) {
      $directory = Join-Path $work "$($baseline.target)/$framework"
      Write-Input (Join-Path $directory 'global.json') (@{ sdk = @{ version = $baseline.sdk; rollForward = 'disable' } } | ConvertTo-Json)
      $target = "$($baseline.target)-windows"
      $settings = if ($framework -eq 'Maui') {
        $target += '10.0.19041.0'
        @"
    <UseMaui>true</UseMaui><SingleProject>true</SingleProject>
    <ApplicationTitle>Willow Quay</ApplicationTitle><ApplicationId>org.playbook.windowsauthoring</ApplicationId>
    <ApplicationDisplayVersion>1.0</ApplicationDisplayVersion><ApplicationVersion>1</ApplicationVersion>
    <SupportedOSPlatformVersion>10.0.17763.0</SupportedOSPlatformVersion>
    <WindowsPackageType>None</WindowsPackageType><WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier><SelfContained>false</SelfContained>
"@
      } elseif ($framework -eq 'Wpf') { '<UseWPF>true</UseWPF><DefineConstants>WPF</DefineConstants>' }
      else { '<UseWindowsForms>true</UseWindowsForms>' }
      $packages = if ($framework -eq 'Maui') { "<ItemGroup><PackageReference Include=`"Microsoft.Maui.Controls`" Version=`"$($baseline.maui)`" /></ItemGroup>" } else { '' }
      Write-Input (Join-Path $directory 'Probe.csproj') @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>$target</TargetFramework><OutputType>WinExe</OutputType>
    <Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>$(if ($baseline.target -eq 'net10.0') { '14.0' } else { '15.0' })</LangVersion>
    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>
$settings
  </PropertyGroup>
  $packages
</Project>
"@
      if ($framework -eq 'Maui') {
        Write-Input (Join-Path $directory 'MauiProgram.cs') $maui
        Write-Input (Join-Path $directory 'Platforms/Windows/App.xaml') '<maui:MauiWinUIApplication x:Class="WindowsAuthoring.WinUI.App" xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:maui="using:Microsoft.Maui" />'
        Write-Input (Join-Path $directory 'Platforms/Windows/App.xaml.cs') @'
namespace WindowsAuthoring.WinUI;
public partial class App : Microsoft.Maui.MauiWinUIApplication
{
    public App() => InitializeComponent();
    protected override Microsoft.Maui.Hosting.MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
'@
      } else { Write-Input (Join-Path $directory 'Program.cs') $desktop }
      $route = [ordered]@{ sample = $directory; framework = $framework; target = $target; intendedSdk = $baseline.sdk; dotnet = $baseline.dotnet; build = 'not-tested'; launch = 'not-tested' }
      Push-Location $directory
      try {
        $route.sdk = Invoke-Dotnet $baseline.dotnet @('--version') (Join-Path $directory 'sdk.log')
        if ($route.sdk -cne $baseline.sdk) { throw 'SDK does not match its pin.' }
        $route.workloads = Invoke-Dotnet $baseline.dotnet @('workload', 'list') (Join-Path $directory 'workloads.log')
        $route.command = @($baseline.dotnet, 'build', 'Probe.csproj', '-c', 'Release')
        $route.build = 'failed'
        $null = Invoke-Dotnet $baseline.dotnet $route.command[1..($route.command.Count - 1)] (Join-Path $directory 'build.log')
        $route.build = 'passed'
        $assets = Get-Content -LiteralPath (Join-Path $directory 'obj/project.assets.json') -Raw | ConvertFrom-Json -AsHashtable
        $route.dependencies = @($assets.libraries.Keys | Sort-Object -CaseSensitive)
        if ($Launch) {
          $executable = Join-Path $directory "bin/Release/$target/$(if ($framework -eq 'Maui') { 'win-x64/' })Probe.exe"
          $receipt = Join-Path (Split-Path $executable) 'launch-result.txt'
          $route.launch = 'failed'
          $runtimeRoot = Split-Path ([IO.Path]::GetFullPath($baseline.dotnet))
          $start = @{ FilePath = $executable; PassThru = $true; WindowStyle = 'Hidden'; WorkingDirectory = (Split-Path $executable)
            Environment = @{ DOTNET_ROOT = $runtimeRoot; DOTNET_ROOT_X64 = $runtimeRoot; DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0' } }
          if ($framework -ne 'Maui') { $start.ArgumentList = @('"' + $receipt + '"') }
          $process = Start-Process @start
          try {
            if (-not $process.WaitForExit(30000)) { throw 'Probe did not close within 30 seconds.' }
            if ($process.ExitCode -ne 0 -or -not (Test-Path -LiteralPath $receipt)) { throw "No successful UI receipt (exit $($process.ExitCode))." }
            $route.launch = 'passed'
            $route.observation = Get-Content -LiteralPath $receipt -Raw
          } finally {
            if (-not $process.HasExited) { $process.Kill($true); $process.WaitForExit() }
            $process.Dispose()
          }
        }
      } catch { $route.limitation = $_.Exception.Message; Write-Warning "$($baseline.target) $framework : $($route.limitation)" }
      finally { Pop-Location }
      $route.inputs = @(Get-ChildItem -LiteralPath $directory -File -Recurse | Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' -and $_.Extension -notin @('.log') } | ForEach-Object {
        @{ path = [IO.Path]::GetRelativePath($work, $_.FullName).Replace('\', '/'); sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant() }
      })
      $routes.Add([pscustomobject]$route)
      Write-Host "$($baseline.target) $framework : build=$($route.build), launch=$($route.launch)"
    }
  }
} finally {
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key, $previous[$key]) }
  foreach ($scope in $paths.Keys) {
    if ([Environment]::GetEnvironmentVariable('PATH', $scope) -cne $paths[$scope]) { throw "$scope PATH changed; inspect shared settings before any correction." }
  }
  $report = [ordered]@{
    recordedAt = [DateTimeOffset]::UtcNow.ToString('O'); kind = 'authoring-preflight'
    host = [Runtime.InteropServices.RuntimeInformation]::OSDescription
    architecture = [Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString(); work = $work
    scriptSha256 = (Get-FileHash -LiteralPath $PSCommandPath -Algorithm SHA256).Hash.ToLowerInvariant()
    sharedInputs = @(Get-ChildItem -LiteralPath $work -Filter '*.props' -File; Get-Item -LiteralPath (Join-Path $work 'Directory.Build.targets')) | ForEach-Object {
      @{ path = $_.Name; sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant() }
    }
    pathUnchanged = $true; routes = $routes
  }
  Write-Input (Join-Path $work 'report.json') ($report | ConvertTo-Json -Depth 30)
  Write-Host "Preflight report and disposable inputs: $work"
}
[pscustomobject]$report
