using Playbook.Samples;

try
{
    RuntimeIdentity.Assert();
    GardenRun.Run();
    return 0;
}
catch (Exception error) { Console.Error.WriteLine(error); return 1; }
