namespace Playbook.Samples;

// snippet:dns-model
public enum RecordKind { A, Aaaa, Mx, Txt, CName, ReverseName, Ns }
public sealed record ZoneRecord(IReadOnlyList<string> Values, TimeSpan Ttl);
public sealed record ZoneAnswer(string Status, IReadOnlyList<ZoneRecord> Records, TimeSpan NegativeTtl);
// endsnippet:dns-model
