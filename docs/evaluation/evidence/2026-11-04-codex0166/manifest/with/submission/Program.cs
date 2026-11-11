using ManifestEvaluation;
Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(Reconciliation.Compare(
    [new("Kite", 0), new("Anchor", 7)], [new("Kite", 2), new("Buoy", 4)])));
