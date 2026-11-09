# Playbook package icon

`playbook-master.png` is the original high-resolution transparent ImageGen raster
master (second candidate approved by Pingu on 9 November 2026).
`../../src/PinguApps.Playbook/icon.png` is its 128 × 128 RGBA package asset.
Only the reduced icon is included in the NuGet package; the master stays here for
future exports. There is no separate vector source.

The concept is an open implementation guide with the PinguApps penguin peeking
over it and one supporting code symbol. References were the BlazorSitemap icon
at `3c3d589533e8ad791508f684ed743d4346c9efa0` and RazorStyle.Build icon at
`aa5973ccabaab7cf87a1d8beba62bada4df7c2fd` in their PinguApps repositories.

Generation used the user's PinguApps family prompt with these substitutions:

- Package: PinguApps.Playbook.
- Purpose: Original offline .NET implementation recipes for coding agents,
  delivered by a locally installed command-line tool over stdio MCP. Agents
  discover relevant guidance by developer goal and target release, then read
  implementation guides, migration steps, examples and applicability constraints.
  The tool provides read-only guidance and does not edit projects.
- Emphasis: A practical reference guide for coding agents.

The follow-up requested the same composition with smoother cutout edges, flat
navy/white/orange/charcoal/cyan/violet fills and transparent margins. The approved
master is retained unchanged. The package export uses Windows System.Drawing
high-quality bicubic interpolation and high-quality pixel offsets at 128px.
Inspect the exported icon at 32px and 128px on both light and dark backgrounds
when replacing it. Installed-tool checks verify the actual archive's icon path,
PNG signature, dimensions, alpha-capable colour type and NuGet size limit.
