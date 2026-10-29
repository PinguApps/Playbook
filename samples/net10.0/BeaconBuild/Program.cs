// snippet:program
using Beacon.Rates;
using Beacon.Rules;

var charge = BerthCharge.Calculate(3, Tariff.CreditsPerCrate);
if (charge != 21) throw new InvalidOperationException("Incorrect quay charge.");
Console.WriteLine($"BEACON: crates=3;credits={charge}");
// endsnippet:program
