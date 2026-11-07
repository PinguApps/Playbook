# Edit nullable values with an explicit converter culture

## Goal

Choose a binding converter culture and preserve validation when users edit, clear and re-enter nullable values.

## Newer approach

Whimbrel records decimal trail distances using French formatting. Set ConverterCulture on Binding so Convert and ConvertBack receive fr-FR. The canonical executable uses the actual MAUI binding engine with an Entry, no handler or renderer. Editor.xaml shows the corresponding x:DataType markup for a compiled binding; it is source-reviewed native integration guidance, not a compiled page in this neutral executable.

Without an explicit value, Binding, typed bindings and MultiBinding use CurrentUICulture for their converter calls. MultiBinding.ConverterCulture supplies its IMultiValueConverter's Convert and ConvertBack culture; configure each child Binding separately if its own converter needs a culture. SourceGen forwards BindingExtension.ConverterCulture into generated typed bindings. This is separate from ordinary unconverted numeric parsing, which still uses CurrentCulture. Do not change global thread culture to repair one editor.

The ordinary newer nullable conversion maps an actual empty string to null. Whitespace is not automatically null, a non-nullable decimal cannot become null, and incomplete numeric input need not overwrite a prior value. Remove a converter only if its sole job was this empty-to-null workaround. Our converter also enforces nonnegative distances and explicit French parsing, so it stays: its empty branch remains its own domain contract and invalid input returns BindableProperty.UnsetValue.

{{snippet:net11-editor}}

## Older approach

The older converter captures the intended CultureInfo itself and explicitly maps empty input to null. It is a valid older solution: do not remove its validation or expect a missing ConverterCulture property to exist. The older package comparison is MAUI 10.0.110, not the historical 10.0.0 source snapshot. Both actual package consumers are compiled and run; no older catalogue edition is added.

{{snippet:net10-program}}

## Migration

1. Inventory each converter's parsing, clearing and validation responsibilities before removing a workaround.
2. Pin the MAUI package/platform composition. Set ConverterCulture=fr-FR on the affected Binding and keep Mode=TwoWay.
3. For compiled markup, provide x:DataType and use a matching MAUI XAML build pipeline. Ordinary SourceGen bindings require no C# expression preview opt-in. C# expressions/x:Code are separate preview features requiring MauiXamlInflator=SourceGen and EnablePreviewFeatures=true; this example uses neither.
4. Test Convert and ConvertBack, actual empty input, whitespace, non-nullable destinations, invalid values and clear/re-entry. The executable checks actual neutral binding behavior; native rendering, keyboard delivery and compiled XAML page generation remain not-tested.
5. Keep a custom converter when it owns domain validation, formatting or culture-specific parsing. Recheck MultiBinding and child converter routes separately when migrating a multi-value editor.

## Canonical samples

{{snippet:net10-program}}

{{snippet:net11-editor}}

{{snippet:net11-program}}

## Constraints

Applicability records runtime, SDK, C# version, packages, workloads, provider, platform, rendering and stability conditions. RC2 published composition and RC3 verification compiler are separate baselines, not a supported combined native installation. No experimental feature opt-in is required for these APIs. Comparisons retain the eight .NET10 foundations. Native exclusions do not establish compatibility or GA readiness.

## Evidence

Run eng/Test-WhimbrelGuidance.ps1 for older Android selected compile/runtime assets and actual old/current MAUI engine checks. Older Apple library compilation is recorded separately. Pinned mobile source/reference review uses eng/Inspect-MobileApiGaps.ps1. Native exclusions apply only to the named samples and conditions; no mock is labelled device evidence. Installed stdio list/get proves offline delivery, not native behavior. PIN-959 and PIN-966 remain open for their own qualifying coverage/GA criteria.

## References

- [Official Binding.cs](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/src/Core/Binding.cs)
- [Official BindingUnitTests.cs](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/tests/Core.UnitTests/BindingUnitTests.cs)
- [Official CompiledBindingMarkup.cs](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/src/SourceGen/CompiledBindingMarkup.cs)
- [Official MultiBinding.cs](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/src/Core/MultiBinding.cs)
- [Official TypedBinding.cs](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/src/Core/TypedBinding.cs)
- [Official Binding.cs](https://raw.githubusercontent.com/dotnet/maui/9d881e94b1c56692e40a16262897677a3bb46fcb/src/Controls/src/Core/Binding.cs)
- [Official BindingExpressionHelper.cs](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/src/Core/BindingExpressionHelper.cs)
- [Official BindingExpressionHelper.cs](https://raw.githubusercontent.com/dotnet/maui/9d881e94b1c56692e40a16262897677a3bb46fcb/src/Controls/src/Core/BindingExpressionHelper.cs)
