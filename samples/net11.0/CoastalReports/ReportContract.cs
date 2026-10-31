// snippet:contract
namespace CoastalReports;

public sealed record SignedSurvey(string Algorithm, string KeyId, ReadOnlyMemory<byte> Payload,
    ReadOnlyMemory<byte> Signature);
public enum SurveyVerification { Valid, Invalid, UnsupportedLegacy }
// endsnippet:contract
