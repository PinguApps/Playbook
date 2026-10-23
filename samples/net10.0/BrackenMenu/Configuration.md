// snippet:host-global-json
{
  "sdk": {
    "version": "10.0.402",
    "rollForward": "disable",
    "allowPrerelease": true
  }
}
// endsnippet:host-global-json

// snippet:client-global-json
{
  "sdk": {
    "version": "10.0.402",
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
