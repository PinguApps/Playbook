# Map environment-variable names to intended configuration keys

## Goal

Ridgeglass Observatory deploys `RIDGEGLASS__Dome__Camera___Gain=23` and wants
`Dome:Camera.Gain`: two underscores delimit sections, three encode a literal dot.
Choose a naming convention explicitly before loading settings. This changes input
keys, independently of which model properties a binder later accepts.

## Older approach

.NET 10's environment provider converts pairs of underscores to colons. It has no
transformation delegate. An original application provider can load the same
bounded convention; retaining it is valid when its policy is required.

{{snippet:older}}

Copy this complete type into a nullable, implicit-usings net10.0/C#14 console
project with `FrameworkReference Include="Microsoft.AspNetCore.App"`. Call
`ObservatorySettings.Load("RIDGEGLASS__")` from the same assembly and dispose the
returned root, for example with `using ConfigurationRoot settings =
(ConfigurationRoot)ObservatorySettings.Load("RIDGEGLASS__");`.
Read `settings["Dome:Camera.Gain"]` and handle a missing value explicitly.
The custom provider snapshots process variables; it does not implement automatic
reload or the framework's special connection-string handling.

## Newer approach

.NET 11's EnvironmentVariablesConfigurationSource exposes
VariableNameTransformation. Select ColonAndDotTransformation to implement the
observatory's convention without maintaining a provider.

{{snippet:newer}}

Use the same call and disposal pattern in a net11.0/C#15 project with the ASP.NET
Core framework reference. No NuGet package is needed on this demonstrated route.
The loaded value remains the string `23`; name transformation does not parse or
validate values. A dot stays inside one configuration segment. It is not a colon
or a binding instruction to traverse a CLR property.

## Migration

1. Choose the intended mapping and inventory existing deployment names. Omitted
   or null VariableNameTransformation uses DefaultTransformation (`__` to `:`).
   Existing defaults therefore do not silently acquire dot semantics on upgrade.
2. Set Prefix and the delegate in AddEnvironmentVariables's source callback.
   The delegate replaces the default transformation, rather than running after
   it. For a custom convention, for example, use
   `source.VariableNameTransformation = name => name.Replace("_SECTION_", ":",
   StringComparison.Ordinal).Replace("__", ":", StringComparison.Ordinal);`.
   Return a non-null string for every input, including the prefix and unrelated
   process names. A null result throws InvalidOperationException while building
   or loading; application exceptions propagate. Keep transformations pure.
3. Transform the prefix under the same convention. Matching and stripping occur
   on transformed names, case-insensitively. `RIDGEGLASS__` and `RIDGEGLASS:` match
   the same prefix; the prefix is removed before publishing a key. A trailing
   delimiter avoids accidentally including a longer neighbouring prefix.
4. Check underscore runs explicitly. A single underscore is literal. Under the
   opt-in mapping, runs of 2/3/4/5/6/7/8 yield `:` / `.` / `._` / `.:` / `..` /
   `.._` / `..:`. Triples are consumed first from left to right. Default mapping
   yields `:` / `:_` / `::` / `::_` / `:::` / `:::_` / `::::` respectively.
   Existing literal dots are preserved. Do not infer four underscores means two
   section delimiters under ColonAndDotTransformation.
5. Reject or eliminate ambiguous deployment names in your deployment validation.
   `Collision__Key` and `Collision:Key` map to the same case-insensitive key.
   The tests accept either supplied value and reject any other value; enumeration
   order within one environment provider is not a stable conflict policy.
   Configuration providers added later override earlier providers. Place explicit
   overrides deliberately and test both provider orders.
6. Execute the real provider with your names on each deployment environment you
   claim. Shells and deployment systems restrict variable names differently.
   The test suite supplies dots/colons directly through child-process environment
   dictionaries; it does not establish that every shell accepts such assignments.

## Constraints and alternatives

The examples use process-local variables only. Do not change PATH or persistent
user/machine settings to try the recipe. Configuration values may contain secrets;
do not print a production environment or its complete configuration for debugging.
Load once or explicitly reload when required; setting an environment variable does
not itself notify this provider. Use the default pair convention when literal dots
are unnecessary, or retain a deliberately limited application mapping.

Framework special connection-string prefixes follow a separate processing path.
This recipe does not advertise or verify those mappings, provider-name entries,
or equivalence between the old custom provider and that path. The original custom
provider is a comparison for ordinary prefixed application settings only. No
package backport or older-target use of the new delegate is established.

## Evidence

`eng/Test-ExtensionContracts.ps1 -Record -VerifySources` builds and runs the actual
ObservatoryVariables .NET10/.NET11 consumers. Exact pins are SDK10.0.402 and
runtime10.0.13, and SDK11.0.100-rc.2.26504.105/runtime11.0.0-rc.2.26504.105.
Both use framework references, empty locked package closures and no workloads,
native tools, external providers, UI/rendering modes or experimental opt-ins.
Application prerequisites are release-family SDK/runtime11, separate from those
measured pins. The audited API has no experimental attribute.

Each consumer starts four real child processes. Checks compare every leaf key
and value for the dot/default/null/custom routes, underscore runs1–8, literal dots,
case-insensitive prefix matching/stripping and lookup, excluded unrelated names,
collisions and both provider orders. Current direct-constructor loading and null
delegate-result rejection also execute. The parent checks that fixture variables
did not leak. Recorded commands, exact inputs/dependencies, compiler/CoreLib,
reference hashes and loaded Extensions identities accompany current compilation
evidence; references match the Extensions source audit.

The ordinary packed tool is installed from a local-only feed/cache and inspected
through real stdio list/get from an unrelated directory. Discovery ID/title/goal,
full canonical Markdown, target11/edition11, applicability and fresh paired
evidence are checked. Target10 has no applicable edition: its code is a comparison,
not a ninth foundation. Retrieval with unusable proxies proves proxy independence,
not network isolation. Windows x64 managed RC2 execution is not other-platform or
final-GA proof. PIN-959/PIN-962 and existing Human gates remain open.
