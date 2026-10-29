using Newtonsoft.Json;

namespace WeatherStation;

// snippet:checks
public static class WeatherChecks
{
    public static void RainfallTotal()
    {
        const string json = """
            {"station":"Gull Point","millimetres":[1.25,0.75,3.0]}
            """;
        Rainfall reading = JsonConvert.DeserializeObject<Rainfall>(json)
            ?? throw new InvalidOperationException("Missing station reading.");
        if (reading.Station != "Gull Point" || reading.Millimetres.Sum() != 5.0m)
        {
            throw new InvalidOperationException("The station's rainfall total changed.");
        }
        Console.WriteLine($"weather-test={reading.Station};rainfall=5.0;runtime={Environment.Version}");
    }

    private sealed record Rainfall(string Station, IReadOnlyList<decimal> Millimetres);
}
// endsnippet:checks
