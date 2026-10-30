// snippet:collector
using System.Text.Json;
using System.Xml;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;

namespace Quay.Collection;

public sealed record CaseObservation(string Phase, Guid Id, string Name, bool IsChild, string? Outcome);

public static class ObserveCase
{
    public static CaseObservation Start(TestCaseEventArgs input)
        => new("start", input.TestCaseId, input.TestCaseName, input.IsChildTestCase, null);

    public static CaseObservation End(TestCaseEndEventArgs input)
        => new("end", input.TestCaseId, input.TestCaseName, input.IsChildTestCase, input.TestOutcome.ToString());
}

[DataCollectorFriendlyName("QuayObservations")]
[DataCollectorTypeUri("datacollector://quay/observations/v1")]
public sealed class QuayCollector : DataCollector
{
    public override void Initialize(XmlElement? configurationElement, DataCollectionEvents events,
        DataCollectionSink dataSink, DataCollectionLogger logger, DataCollectionEnvironmentContext? environmentContext)
    {
        ArgumentNullException.ThrowIfNull(events);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(environmentContext);
        events.SessionStart += (_, _) => logger.LogWarning(environmentContext.SessionDataCollectionContext,
            "QUAY_COLLECTOR:" + typeof(TestCaseEventArgs).Assembly.FullName + ";location=" + typeof(TestCaseEventArgs).Assembly.Location);
        // Own immutable values, never a mutable TestElement or event-args reference.
        events.TestCaseStart += (_, input) => logger.LogWarning(input.Context,
            "QUAY_EVENT:" + JsonSerializer.Serialize(ObserveCase.Start(input)));
        events.TestCaseEnd += (_, input) => logger.LogWarning(input.Context,
            "QUAY_EVENT:" + JsonSerializer.Serialize(ObserveCase.End(input)));
    }
}
// endsnippet:collector
