// snippet:ceremony
using Microsoft.Maui.Authentication;

namespace HeronKeys;

// The caller obtains fresh options from its RP server and sends the returned
// JSON back to that same server. Returning JSON never establishes a session.
public sealed class Ceremony(IPasskeys passkeys)
{
    public async Task<string> RegisterAsync(string serverOptions, CancellationToken cancellationToken)
    {
        if (!passkeys.IsSupported) { throw new PlatformNotSupportedException("Use the account's recovery sign-in route."); }
        var response = await passkeys.CreateAsync(new PasskeyCreationOptions(serverOptions), cancellationToken);
        return response.ToString();
    }

    public async Task<string> SignInAsync(string serverOptions, CancellationToken cancellationToken)
    {
        if (!passkeys.IsSupported) { throw new PlatformNotSupportedException("Use the account's recovery sign-in route."); }
        var response = await passkeys.AssertAsync(new PasskeyRequestOptions(serverOptions), cancellationToken);
        return response.ToString();
    }
}
// endsnippet:ceremony
