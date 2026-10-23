// snippet:n11-global-json
{
  "sdk": {
    "version": "11.0.100-rc.2.26504.105",
    "rollForward": "disable",
    "allowPrerelease": true
  }
}
// endsnippet:n11-global-json

// snippet:n11-directory-build-props
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
// endsnippet:n11-directory-build-props

// snippet:n11-directory-build-targets
<Project />
// endsnippet:n11-directory-build-targets

// snippet:n11-directory-packages-props
<Project>
  <PropertyGroup><ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally><CentralPackageVersionOverrideEnabled>false</CentralPackageVersionOverrideEnabled></PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="Microsoft.AspNetCore.Components.WebAssembly" Version="[11.0.0-rc.2.26504.105]" />
    <PackageVersion Include="Microsoft.AspNetCore.Components.Gateway" Version="[11.0.0-rc.2.26504.105]" />
    <PackageVersion Include="Microsoft.Extensions.ServiceDiscovery" Version="[10.10.0]" />
    <PackageVersion Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="[1.19.1]" />
    <PackageVersion Include="OpenTelemetry.Extensions.Hosting" Version="[1.19.1]" />
    <PackageVersion Include="OpenTelemetry.Instrumentation.Http" Version="[1.19.0]" />
    <PackageVersion Include="OpenTelemetry.Instrumentation.Runtime" Version="[1.19.0]" />
  </ItemGroup>
</Project>
// endsnippet:n11-directory-packages-props

// snippet:n11-client-appsettings-json
{
  "Gateway": {
    "HttpsRedirection": {
      "Enabled": false
    },
    "Telemetry": {
      "Enabled": false
    }
  },
  "services": {
    "quaybackend": {
      "http": [
        "http://127.0.0.1:5914"
      ]
    }
  },
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
            "Address": "http://quaybackend/"
          }
        }
      }
    }
  },
  "ClientApps": {
    "app": {
      "ConfigEndpointPath": "/_blazor/_configuration",
      "ConfigResponse": "{\"webAssembly\":{\"environment\":{\"services__quays__http__0\":\"http://localhost:5912\"}}}"
    }
  }
}
// endsnippet:n11-client-appsettings-json
