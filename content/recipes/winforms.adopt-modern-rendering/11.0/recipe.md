# Adopt modern Windows Forms appearance deliberately

## Goal

A Bracken station preferences screen needs clearer controls while preserving a retained editor's appearance. Keep its existing settings model and change rendering independently.

## Older approach

The .NET 10 screen uses supported styled controls and automatic layout. It keeps the same station value, report checkbox and TreeView node height. There is no corresponding Net11 renderer or ToggleSwitch appearance in this target. Existing classic controls remain a valid compatibility choice.

## Newer approach

{{snippet:n11-program-cs}}

Enable visual styles, then select Net11 before creating windows. Defaults are selected once; repeating the same selection is allowed, changing it is rejected. Latest follows the runtime's newest stable renderer; choose Net11 for a deliberate version. Classic retains themed controls; Disabled turns visual styles off. Dark mode is independent.

Children request Inherit; that getter is not their effective renderer. An explicit child selection survives parent changes until reset to Inherit. Inherit is invalid as the application default. High Contrast forces classic styled rendering; disabled stays disabled.

Use automatic layout because modern metrics change. TextBoxBase padding affects text only in modern modes. ToolStrip and DataGridView retain classic fallbacks; their respective System.Windows.Forms.ToolStripModernRendering and System.Windows.Forms.DataGridViewModernRendering switches remove those fallbacks but neither enable styles nor select Net11. PropertyGrid remains classic.

ToggleSwitch is two-state CheckBox/RadioButton appearance with existing state/binding/group semantics. ThreeState, Classic, Disabled and High Contrast do not provide the modern toggle. FlatStyle.System does not override it.

SortGlyphColor customizes the header arrow; Color.Empty chooses contrast automatically. PrintPreviewControl.ForeColor affects empty-preview messaging and explicitly assigned page fill, not document ink. Preview-only NodeLeading users migrate to ItemHeight; .NET10 applications never had NodeLeading.

## Migration

1. Keep retained controls Classic and adopt automatic sizing.
2. Inspect editable bounds, long labels and DPI.
3. Test accessibility transitions before promising visual compatibility. Keep composite opt-ins off until checked.

## Canonical samples

### n10-brackensettings-csproj
{{snippet:n10-brackensettings-csproj}}

### n10-directory-build-props
{{snippet:n10-directory-build-props}}

### n10-directory-build-targets
{{snippet:n10-directory-build-targets}}

### n10-directory-packages-props
{{snippet:n10-directory-packages-props}}

### n10-global-json
{{snippet:n10-global-json}}

### n10-program-cs
{{snippet:n10-program-cs}}

### n11-brackensettings-csproj
{{snippet:n11-brackensettings-csproj}}

### n11-directory-build-props
{{snippet:n11-directory-build-props}}

### n11-directory-build-targets
{{snippet:n11-directory-build-targets}}

### n11-directory-packages-props
{{snippet:n11-directory-packages-props}}

### n11-global-json
{{snippet:n11-global-json}}

### n11-program-cs
{{snippet:n11-program-cs}}

## Constraints

Windows desktop, STA execution, SDK10.0.402/C#14 and SDK11.0.100-rc.2.26504.105/C#15. No added NuGet package, service account or remote provider. The current edition targets .NET11; the older sample is comparison evidence, not a .NET10 foundation or backport. Framework-reference and runtime assets must match their declared targets. No experimental API opt-in is required by these compiled samples. RC2 execution does not establish GA verification.

## Evidence

All four canonical projects compile and execute on Windows x64 with runtime10.0.13 or runtime11RC2. Saved verification-evidence.json records exact inputs and commands. See metadata for each sample's actual scope. No global Windows settings are changed. No external clipboard-owner failure, pixel-level rendering, High Contrast transition, Designer round trip or GA behavior is claimed. Existing Human and final-release gates remain open.

## References

[Official RC2 desktop contracts](https://raw.githubusercontent.com/dotnet/core/60871330fce2b3da383461be027ebdf3ce30f8fb/release-notes/11.0/preview/rc2/winforms.md).

