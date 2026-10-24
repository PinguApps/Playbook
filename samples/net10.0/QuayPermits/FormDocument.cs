// snippet:complete
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Antiforgery;
namespace QuayPermits;

public sealed record ProtectionMode(bool KeepTokens);
public sealed record PermitRequest(string Cargo);

public static class FormDocument
{
    public static string Create(string action, HttpContext context, IAntiforgery antiforgery, bool keepTokens)
    {
        string tokenField = "";
        if (keepTokens)
        {
            AntiforgeryTokenSet tokens = antiforgery.GetAndStoreTokens(context);
            tokenField = $"<input type=\"hidden\" name=\"{HtmlEncoder.Default.Encode(tokens.FormFieldName)}\" value=\"{HtmlEncoder.Default.Encode(tokens.RequestToken!)}\">";
        }
        return $"<!DOCTYPE html><html lang=\"en\"><title>Quay permit</title><form method=\"post\" action=\"{HtmlEncoder.Default.Encode(action)}\">{tokenField}<label>Cargo <input name=\"cargo\" value=\"lanterns\"></label><button>Submit permit</button></form></html>";
    }
}

public sealed class PermitAdditionalData(bool reject) : IAntiforgeryAdditionalDataProvider
{
    public string GetAdditionalData(HttpContext context) => "quay-permit-v1";
    public bool ValidateAdditionalData(HttpContext context, string additionalData) =>
        !reject && additionalData == "quay-permit-v1";
}
// endsnippet:complete
