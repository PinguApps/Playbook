// snippet:program
#pragma warning disable IL3000 // This diagnostic is the observed difference: a bundled assembly has no file location.
Console.WriteLine($"manifest=PIER-42;assembly={typeof(Program).Assembly.Location};base={AppContext.BaseDirectory}");
// endsnippet:program
