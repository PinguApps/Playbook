using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Resources;

namespace Playbook.Samples;

// snippet:model
public sealed class VisitRequest
{
    [Required] public string Pass { get; set; } = "open";
    [Display(Name = nameof(Messages.PartySize), ResourceType = typeof(Messages)), PartyLimit(6)]
    public int Guests { get; set; } = 2;
}
// endsnippet:model

// snippet:formatter
public static class Messages
{
    private static readonly ResourceManager Resources = new("Playbook.Samples.VisitRequest", typeof(Messages).Assembly);
    public static string PartySize => Resources.GetString("Party size", CultureInfo.CurrentUICulture)!;
    public static string PartyLimit => Resources.GetString("VisitRequest_Guests_PartyLimitAttribute_Error", CultureInfo.CurrentUICulture)!;
}

public sealed class PartyLimitAttribute(int maximum) : ValidationAttribute
{
    public override bool IsValid(object? value) => value is int count && count is > 0 && count <= maximum;
    public override string FormatErrorMessage(string name) =>
        string.Format(CultureInfo.CurrentCulture, Messages.PartyLimit, name, maximum);
}
// endsnippet:formatter

// snippet:checks
public static class BookingChecks
{
    public static async Task<Dictionary<string, string[]>> CheckAsync(
        VisitRequest request, IServiceProvider services, CancellationToken token)
    {
        List<ValidationResult> results = [];
        Validator.TryValidateObject(request, new(request, services, null), results, true);
        if (!string.IsNullOrWhiteSpace(request.Pass) &&
            !await services.GetRequiredService<VisitRule>().IsOpenAsync(request.Pass, token))
            results.Add(new("This visit is closed.", [nameof(request.Pass)]));
        return results.SelectMany(result => result.MemberNames.Select(member => (member, result.ErrorMessage!)))
            .GroupBy(pair => pair.member).ToDictionary(group => group.Key, group => group.Select(pair => pair.Item2).ToArray());
    }
}
// endsnippet:checks
