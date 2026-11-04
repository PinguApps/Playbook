using System.Security.Cryptography;
using Playbook.Samples;

RuntimeIdentity.Assert();
if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Windows CNG property case only");
byte[] input = [17, 23, 41];
#if NET11_0_OR_GREATER
CngProperty property = new("RookeryCalibration", input.AsSpan(), CngPropertyOptions.CustomProperty);
#else
CngProperty property = new("RookeryCalibration", input, CngPropertyOptions.CustomProperty);
#endif
input[0] = 99;
byte[] value = property.GetValue()!;
if (value[0] != 17) throw new Exception("Constructor did not copy input");
value[1] = 99;
if (property.GetValue()![1] != 23) throw new Exception("Getter exposed owned buffer");
Console.WriteLine("PASS Windows CngProperty input and getter copies; span constructor only on .NET11; no key/provider operation claimed");
