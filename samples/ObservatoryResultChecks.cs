using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using Observatory;
static void Check(bool ok, string claim) { if (!ok) throw new Exception(claim); Console.WriteLine("PASS " + claim); }
Console.WriteLine(RuntimeInformation.FrameworkDescription);
#if NET11_0_OR_GREATER
static Decimal128 P(string s) => Decimal128.Parse(s, CultureInfo.InvariantCulture);
var x = P("0.9999999999999999999999999999999999");
var acos = Decimal128.Acos(x);
Console.WriteLine($"INPUT {x} Acos={acos} Log={Decimal128.Log(x)} SinPi={Decimal128.SinPi(x)}");
if (args.Contains("--observe")) return;
// Independent leading term: acos(1-d)=sqrt(2d)*(1+d/12+O(d^2)).
// At d=1e-34 omitted correction <2e-52; decimal constant sqrt(2) rounded to34 digits.
Check(Results.Accept(acos, P("1.414213562373095048801688724209698e-17"), P("1e-49"), P("1e-32")), "cancellation-sensitive independent reference bound");
Check(!Results.Accept(acos, P("1.414250203437893161687265742266675e-17"), P("1e-49"), P("1e-32")), "stale RC1 expected output deliberately rejected");
Check(Decimal128.Acos(P("0.5")) == Decimal128.Acos(P("0.5000")), "alternate exact input representations");
Check(Decimal128.SinPi(P("0.5")) == Decimal128.One, "ordinary exact sin(pi/2)");
Check(Decimal32.SinPi((Decimal32)0.5) == Decimal32.One && Decimal64.Log(Decimal64.One) == Decimal64.Zero, "Decimal32/64 ordinary controls");
Check(Decimal128.Log((Decimal128)8, (Decimal128)2) == (Decimal128)3, "Log with explicit base");
Check(!Results.Accept(Decimal128.NaN, Decimal128.NaN, P("1e-49"), P("1e-32")) &&
      !Results.Accept(Decimal128.PositiveInfinity, Decimal128.PositiveInfinity, P("1e-49"), P("1e-32")), "nonfinite results rejected");
Check(Results.Accept(Decimal128.NegativeZero, Decimal128.Zero, Decimal128.Zero, Decimal128.Zero), "numeric comparison intentionally equates signed zero");
#else
double x = 1 - 1e-12;
double value = Math.Acos(x);
Check(Results.Accept(value, Math.Sqrt(2 * (1 - x)), 1e-15, 1e-12), "binary double route and bounded leading term");
Check(!Results.Accept(value, value + 0.01, 1e-15, 1e-12), "unacceptable binary drift rejected");
Check(!Results.Accept(double.NaN, double.NaN, 1e-15, 1e-12), "nonfinite binary rejected");
Console.WriteLine("Math/double is a different representation; not old IEEE decimal emulation.");
#endif
Check(!Results.Accept(1.7e308, -1.7e308, 1e-15, 2.0), "overflowed tolerance fails closed");
var history = new Results.Saved("angle:exact-source-text", "historical-v1", "preserved");
var proposal = Results.Propose(history, "recomputed-v2", "candidate");
Check(history.Output == "preserved" && proposal.InputId == history.InputId && proposal.AlgorithmVersion != history.AlgorithmVersion, "versioned disposable proposal preserves original and exact ID");
Check(history != proposal, "rollback selects unchanged historical record");
