// snippet:host-global-json
{
  "sdk": {
    "version": "11.0.100-rc.2.26504.105",
    "rollForward": "disable",
    "allowPrerelease": true
  }
}
// endsnippet:host-global-json

// snippet:client-global-json
{
  "sdk": {
    "version": "11.0.100-rc.2.26504.105",
    "rollForward": "disable",
    "allowPrerelease": true
  }
}
// endsnippet:client-global-json

// snippet:host-_imports-razor
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using static Microsoft.AspNetCore.Components.Web.RenderMode
@using BrackenMenu.Host
@using BrackenMenu.Host.Components.Layout
// endsnippet:host-_imports-razor

// snippet:client-_imports-razor
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using static Microsoft.AspNetCore.Components.Web.RenderMode
@using BrackenMenu.Client
@using BrackenMenu.Client.Layout
// endsnippet:client-_imports-razor

// snippet:host-wwwroot-menu-css
body { font: 1rem system-ui; max-width: 48rem; margin: 2rem auto; padding: 1rem; }
bracken-menu { display: block; padding: 1rem; border: 2px solid #31533c; }
.nav-scrollable { display: none; padding: 1rem; }
input:checked ~ .nav-scrollable { display: block; }
a, button, label { margin: .5rem; }
#outside { padding: 1rem; border: 1px solid #777; }
// endsnippet:host-wwwroot-menu-css

// snippet:directory-build-props
<Project>
  <PropertyGroup>
    <Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors><Deterministic>true</Deterministic>
    <IncludeSourceRevisionInInformationalVersion>false</IncludeSourceRevisionInInformationalVersion>
    <EnableSourceLink>false</EnableSourceLink><EnableSourceControlManagerQueries>false</EnableSourceControlManagerQueries>
    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile><ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
  </PropertyGroup>
</Project>
// endsnippet:directory-build-props

// snippet:directory-build-targets
<Project />
// endsnippet:directory-build-targets

// snippet:directory-packages-props
<Project><PropertyGroup><ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally></PropertyGroup></Project>
// endsnippet:directory-packages-props
