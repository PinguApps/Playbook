// snippet:program
using Playbook.Samples;
Console.WriteLine(BeaconFrame.Decode(new byte[] { 0x35, 0x12 })); // 4661
// endsnippet:program
UnsafeBoundaryChecks.Run();
