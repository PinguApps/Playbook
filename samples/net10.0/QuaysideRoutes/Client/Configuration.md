// snippet:n10-global-json
{
  "sdk": {
    "version": "10.0.402",
    "rollForward": "disable",
    "allowPrerelease": true
  }
}
// endsnippet:n10-global-json

// snippet:n10-directory-build-props
<Project>
  <PropertyGroup>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <Deterministic>true</Deterministic>
    <IncludeSourceRevisionInInformationalVersion>false</IncludeSourceRevisionInInformationalVersion>
    <EnableSourceLink>false</EnableSourceLink>
    <EnableSourceControlManagerQueries>false</EnableSourceControlManagerQueries>
    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>
  </PropertyGroup>
</Project>
// endsnippet:n10-directory-build-props

// snippet:n10-directory-build-targets
<Project />
// endsnippet:n10-directory-build-targets

// snippet:n10-directory-packages-props
<Project>
  <PropertyGroup><ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally><CentralPackageVersionOverrideEnabled>false</CentralPackageVersionOverrideEnabled></PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="Microsoft.AspNetCore.Components.WebAssembly" Version="[10.0.13]" />
    <PackageVersion Include="Microsoft.AspNetCore.Components.WebAssembly.Server" Version="[10.0.13]" />
    <PackageVersion Include="Yarp.ReverseProxy" Version="[2.3.0]" />
  </ItemGroup>
</Project>
// endsnippet:n10-directory-packages-props

// snippet:n10-host-appsettings-json
{
  "ReverseProxy": {
    "Routes": {
      "quays": {
        "ClusterId": "quays",
        "Match": {
          "Path": "/api/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "quays": {
        "Destinations": {
          "local": {
            "Address": "http://127.0.0.1:5913/"
          }
        }
      }
    }
  }
}
// endsnippet:n10-host-appsettings-json
