// snippet:survey-request
using System.Net;

internal static class SurveyRequest
{
    public static HttpRequestMessage Create(Uri trustedSurveyUri) => new(HttpMethod.Get, trustedSurveyUri)
    {
        Version = HttpVersion.Version11,
        VersionPolicy = HttpVersionPolicy.RequestVersionExact
    };
}
// endsnippet:survey-request
