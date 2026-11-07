// snippet:complete
using System.Globalization;
using Microsoft.Maui.Controls;
using WhimbrelEdit;

var model = new TrailReading();
var entry = new Entry { BindingContext = model };
entry.SetBinding(Entry.TextProperty, new Binding(nameof(TrailReading.Distance), BindingMode.TwoWay));
entry.Text = "";
if (model.Distance is not null) throw new Exception("Clearing retained the previous distance.");
entry.Text = "12";
if (model.Distance != 12) throw new Exception("Re-entry failed.");
entry.Text = " ";
if (model.Distance != 12) throw new Exception("Whitespace unexpectedly cleared the value.");
var converter = new DistanceConverter();
entry.SetBinding(Entry.TextProperty, new Binding(nameof(TrailReading.Distance), BindingMode.TwoWay)
{
    Converter = converter,
    ConverterCulture = CultureInfo.GetCultureInfo("fr-FR")
});
entry.Text = "14,5";
if (model.Distance != 14.5m || converter.LastCulture != "fr-FR") throw new Exception("Explicit converter culture failed.");
entry.Text = "invalid";
if (model.Distance != 14.5m) throw new Exception("Invalid input replaced the last valid reading.");
entry.Text = "";
if (model.Distance is not null) throw new Exception("Custom converter clearing failed.");
var required = new Entry { BindingContext = model };
required.SetBinding(Entry.TextProperty, new Binding(nameof(TrailReading.RequiredDistance), BindingMode.TwoWay));
required.Text = "";
if (model.RequiredDistance != 8) throw new Exception("Empty text changed a non-nullable distance.");
var defaultConverter = new DistanceConverter();
required.SetBinding(Entry.TextProperty, new Binding(nameof(TrailReading.RequiredDistance)) { Converter = defaultConverter });
if (defaultConverter.LastCulture != CultureInfo.CurrentUICulture.Name) throw new Exception("Unexpected default converter culture.");
model.Distance = 8;
var multiConverter = new DistanceMultiConverter();
entry.SetBinding(Entry.TextProperty, new MultiBinding
{
    Mode = BindingMode.TwoWay,
    ConverterCulture = CultureInfo.GetCultureInfo("fr-FR"),
    Converter = multiConverter,
    Bindings = { new Binding(nameof(TrailReading.Distance)) }
});
entry.Text = "16,5";
if (model.Distance != 16.5m || multiConverter.LastCulture != "fr-FR") throw new Exception("MultiBinding converter culture failed.");
Console.WriteLine("PASS actual MAUI bindings: clear, re-entry, whitespace, culture and validation; no native UI rendering.");

namespace WhimbrelEdit
{
public sealed class TrailReading
{
    public decimal? Distance { get; set; } = 8;
    public decimal RequiredDistance { get; set; } = 8;
}

public sealed class DistanceConverter : IValueConverter
{
    public string? LastCulture { get; private set; }
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        LastCulture = culture.Name;
        return value is decimal distance ? distance.ToString(culture) : "";
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        LastCulture = culture.Name;
        if (value is "") return null!;
        return value is string text && decimal.TryParse(text, NumberStyles.Number, culture, out var distance) && distance >= 0
            ? distance : BindableProperty.UnsetValue;
    }
}
public sealed class DistanceMultiConverter : IMultiValueConverter
{
    public string? LastCulture { get; private set; }
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        LastCulture = culture.Name;
        return values[0] is decimal distance ? distance.ToString(culture) : "";
    }
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        LastCulture = culture.Name;
        return value is string text && decimal.TryParse(text, NumberStyles.Number, culture, out var distance)
            ? [distance] : [BindableProperty.UnsetValue];
    }
}
}
// endsnippet:complete
