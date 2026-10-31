using ShoreCalibration;

if (Calibration.ConvertReading(60) != 42) { throw new Exception("Calibration mismatch"); }
Console.WriteLine("PASS managed comparison: 60 -> 42; no CoreCLR WebAssembly or native linking claim");
