using Playbook.Samples;

try
{
    RuntimeIdentity.Assert();
    PlanetariumChecks.Run();
    return 0;
}
catch (Exception error) { Console.Error.WriteLine(error); return 1; }
