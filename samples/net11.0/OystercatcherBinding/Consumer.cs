// snippet:complete
namespace Oystercatcher;
public static class PermitDisplay
{
    public static string Describe(string code)
    {
        using SurveyPermit? permit = SurveyPermit.TryCreate(code);
        return permit is null ? "Station rejected" : "Survey permitted";
    }
}
// endsnippet:complete
