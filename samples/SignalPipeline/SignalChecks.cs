using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using Playbook.Samples;

Console.WriteLine($"runtime={RuntimeInformation.FrameworkDescription}; arch={RuntimeInformation.ProcessArchitecture}; os={RuntimeInformation.OSDescription}");
Console.WriteLine($"COMPLEX ASSET {typeof(Complex).Assembly.Location}; sha256={Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(typeof(Complex).Assembly.Location)))}");
double Number(string text) => double.Parse(text, CultureInfo.InvariantCulture);
var infinity = Number("Infinity");
var nan = Number("NaN");
var negativeZero = Number("-0");
var carrier = new Complex(infinity, -infinity);
var gain = new Complex(2, 0);
var finite = new Complex(6, -8);
var cases = new (string Name, Complex Value)[]
{
    ("finite-product", finite * gain),
    ("infinite-product", carrier * gain),
    ("multiply-method", Complex.Multiply(carrier, gain)),
    ("finite-quotient", finite / gain),
    ("infinite-quotient", finite / carrier),
    ("zero-divisor", finite / Complex.Zero),
    ("divide-method", Complex.Divide(finite, Complex.Zero)),
    ("undefined-quotient", Complex.Zero / Complex.Zero),
    ("reciprocal-infinity", Complex.Reciprocal(carrier)),
    ("reciprocal-zero", Complex.Reciprocal(Complex.Zero)),
    ("power-real", Complex.Pow(new Complex(infinity, 0), 2)),
    ("power-complex", Complex.Pow(new Complex(infinity, 0), new Complex(2, 0))),
    ("sqrt-infinity", Complex.Sqrt(new Complex(-infinity, -2))),
    ("exp-infinity", Complex.Exp(new Complex(infinity, negativeZero))),
    ("log-infinity", Complex.Log(carrier)),
    ("log-base", Complex.Log(carrier, 2)),
    ("log10-infinity", Complex.Log10(carrier)),
    ("sin-infinity", Complex.Sin(new Complex(negativeZero, infinity))),
    ("cos-infinity", Complex.Cos(new Complex(negativeZero, infinity))),
    ("tan-infinity", Complex.Tan(new Complex(2, infinity))),
    ("sinh-infinity", Complex.Sinh(new Complex(infinity, negativeZero))),
    ("cosh-infinity", Complex.Cosh(new Complex(-infinity, infinity))),
    ("tanh-infinity", Complex.Tanh(new Complex(infinity, 2))),
    ("asin-infinity", Complex.Asin(new Complex(infinity, -2))),
    ("acos-infinity", Complex.Acos(new Complex(-infinity, nan))),
    ("atan-infinity", Complex.Atan(new Complex(-infinity, 2))),
    ("sqrt-signed-zero", Complex.Sqrt(new Complex(-9, negativeZero))),
    ("exp-signed-zero", Complex.Exp(new Complex(0, negativeZero))),
    ("log-signed-zero", Complex.Log(new Complex(-3, negativeZero)))
};
foreach (var (name, value) in cases)
{
    Console.WriteLine($"{name}: ({value.Real:R},{value.Imaginary:R}); bits=({BitConverter.DoubleToInt64Bits(value.Real):X16},{BitConverter.DoubleToInt64Bits(value.Imaginary):X16}); class={SignalPolicy.Classify(value)}");
}
Complex Result(string name) => cases.Single(item => item.Name == name).Value;
void Check(bool condition, string name)
{
    if (!condition) throw new InvalidOperationException(name);
    Console.WriteLine($"PASS {name}");
}
Check(Result("finite-product") == new Complex(12, -16), "finite multiplication");
Check(Result("finite-quotient") == new Complex(3, -4), "finite division");
Check(Complex.Reciprocal(new Complex(2, 0)) == new Complex(0.5, 0) && Complex.Abs(finite) == 10, "finite reciprocal and magnitude");
Check(Complex.Pow(new Complex(2, 0), 2).Real == 4 && Complex.Sqrt(new Complex(9, 0)) == new Complex(3, 0), "finite power and root");
Check(SignalPolicy.Classify(finite) == SignalState.Finite, "finite routed downstream");
Check(SignalPolicy.Classify(new Complex(nan, 3)) == SignalState.Undefined, "NaN without infinity rejected");
Check(SignalPolicy.Classify(new Complex(infinity, nan)) == SignalState.DirectedInfinity, "infinity takes precedence over companion NaN");
Check(SignalPolicy.Classify(Complex.Zero / Complex.Zero) == SignalState.Undefined, "zero over zero undefined");
var magnitude = Complex.Abs(new Complex(infinity, nan));
Console.WriteLine($"magnitude(infinity,NaN)={magnitude:R}");
#if NET11_0
Check(double.IsPositiveInfinity(Result("infinite-product").Real) && double.IsNegativeInfinity(Result("infinite-product").Imaginary), "product retains direction");
Check(Result("multiply-method") == Result("infinite-product") && Result("divide-method") == Result("zero-divisor"), "operator and named methods agree");
Check(double.IsPositiveInfinity(Result("zero-divisor").Real) && double.IsNegativeInfinity(Result("zero-divisor").Imaginary), "zero divisor retains direction");
Check(double.IsPositiveInfinity(magnitude), "magnitude prioritizes infinity");
Check(Result("atan-infinity").Real == -Math.PI / 2 && Result("atan-infinity").Imaginary == 0, "inverse limit");
Check(Result("sqrt-signed-zero").Imaginary == -3, "root side selected by signed zero");
Check(double.IsPositiveInfinity(Result("cosh-infinity").Real) && double.IsNaN(Result("cosh-infinity").Imaginary), "hyperbolic mixed infinity NaN");
Check(double.IsNegative(Result("sin-infinity").Real) && Result("sin-infinity").Real == 0 && double.IsPositiveInfinity(Result("sin-infinity").Imaginary), "trigonometric signed zero and infinity");
#else
Check(double.IsNaN(Result("infinite-product").Real) && double.IsNaN(Result("infinite-product").Imaginary), "previous product loses direction");
Check(SignalPolicy.Classify(Result("zero-divisor")) == SignalState.Undefined, "previous zero divisor undefined");
Check(Result("sqrt-signed-zero").Imaginary == 3, "previous root ignores zero sign");
#endif
Check(Result("exp-signed-zero").Imaginary == 0 && double.IsNegative(Result("exp-signed-zero").Imaginary), "exponential preserves negative zero");
Check(Result("reciprocal-zero") == Complex.Zero, "reciprocal zero measured on both artifacts");
// A zero quotient component's sign is deliberately observed above, never asserted.
Console.WriteLine("PASS signal pipeline checks; nongeneric Complex throughout");
