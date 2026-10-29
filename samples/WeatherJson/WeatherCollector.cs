using System.Xml;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using Newtonsoft.Json.Linq;

namespace WeatherStation;

// snippet:collector
[DataCollectorFriendlyName("WeatherStation")]
[DataCollectorTypeUri("datacollector://weatherstation/json/v1")]
public sealed class WeatherCollector : DataCollector
{
    public override void Initialize(XmlElement? configurationElement, DataCollectionEvents events,
        DataCollectionSink dataSink, DataCollectionLogger logger, DataCollectionEnvironmentContext? environmentContext)
    {
        ArgumentNullException.ThrowIfNull(environmentContext);
        JObject settings = JObject.Parse("""{"sensor":"rain-gauge","intervalSeconds":60}""");
        if ((int?)settings["intervalSeconds"] != 60) { throw new InvalidOperationException("Wrong interval."); }
        events.SessionStart += (_, _) => logger.LogWarning(environmentContext.SessionDataCollectionContext,
            "weather-collector=loaded;sensor=rain-gauge");
    }
}
// endsnippet:collector
