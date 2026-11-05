// snippet:collector
using System.Text;
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
            Describe(ObserveCase.Start(input)));
        events.TestCaseEnd += (_, input) => logger.LogWarning(input.Context,
            Describe(ObserveCase.End(input)));
    }

    // The collector host owns its serializer dependencies. Keep this diagnostic
    // protocol on core APIs; Base64 preserves arbitrary test names on one line.
    private static string Describe(CaseObservation observation)
        => $"QUAY_EVENT:{observation.Phase}|{observation.Id}|{Convert.ToBase64String(Encoding.UTF8.GetBytes(observation.Name))}|{observation.IsChild}|{observation.Outcome}";
}
// endsnippet:collector
