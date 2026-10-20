using Playbook.Samples;

#if NET11_0_OR_GREATER
using Labels = Playbook.Samples.IObservationSource;
#else
using Labels = Playbook.Samples.ObservationLabels;
#endif

IObservationSource source = new RoofSensor();
if (ObservationReport.Label(source, 7) != "ROOF-007" ||
    Labels.CreateLabel(" dome ", 42) != "DOME-042" ||
    Labels.CreateLabel("west", 0) != "WEST-000")
{
    throw new InvalidOperationException("Observation labels changed.");
}

try
{
    Labels.CreateLabel(" ", 1);
    throw new InvalidOperationException("A blank station was accepted.");
}
catch (ArgumentException) { }

try
{
    Labels.CreateLabel("roof", -1);
    throw new InvalidOperationException("A negative sequence was accepted.");
}
catch (ArgumentOutOfRangeException) { }

#if NET11_0_OR_GREATER
System.Reflection.MethodInfo helper = typeof(IObservationSource).GetMethod(nameof(IObservationSource.CreateLabel))!;
if (!helper.IsStatic || helper.IsVirtual || helper.IsAbstract)
{
    throw new InvalidOperationException("The helper became a dispatch contract.");
}
#endif

Console.WriteLine($"PASS: static interface helper labels, validation and implementer obligations; runtime {Environment.Version}");
