// snippet:complete
using System.Globalization;
using Microsoft.Maui.Controls;
using WhimbrelEdit;

var model = new TrailReading();
var entry = new Entry { BindingContext = model };
entry.SetBinding(Entry.TextProperty, new Binding(nameof(TrailReading.Distance), BindingMode.TwoWay)
{
    Converter = new DistanceConverter(CultureInfo.GetCultureInfo("fr-FR"))
});
entry.Text = "14,5";
if (model.Distance != 14.5m) throw new Exception("Explicit converter parsing failed.");
entry.Text = "";
if (model.Distance is not null) throw new Exception("Clearing retained the previous reading.");
entry.Text = "12";
if (model.Distance != 12) throw new Exception("Re-entry failed.");
entry.Text = "invalid";
if (model.Distance != 12) throw new Exception("Invalid input replaced the last valid reading.");
Console.WriteLine("PASS older MAUI converter culture, clearing, re-entry and validation; no native UI rendering.");

namespace WhimbrelEdit
{
public sealed class TrailReading
{
    public decimal? Distance { get; set; } = 8;
}

public sealed class DistanceConverter(CultureInfo selectedCulture) : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is decimal distance ? distance.ToString(selectedCulture) : "";
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is "") return null!;
        return value is string text && decimal.TryParse(text, NumberStyles.Number, selectedCulture, out var distance) && distance >= 0
            ? distance : BindableProperty.UnsetValue;
    }
}
}
// endsnippet:complete
