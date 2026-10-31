// snippet:program
using ShoreCalibration;

if (Calibration.ConvertReading(60) != 42) { throw new Exception("Native calibration mismatch"); }
Console.WriteLine("PASS native shore calibration: 60 -> 42");
// endsnippet:program
