// snippet:complete
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BeaconKeys;

// An older-target application adapter, not a change to shared policy.
public sealed class GuardedSignInManager(
    UserManager<IdentityUser> users,
    IHttpContextAccessor accessor,
    IUserClaimsPrincipalFactory<IdentityUser> claims,
    IOptions<IdentityOptions> options,
    ILogger<SignInManager<IdentityUser>> logger,
    IAuthenticationSchemeProvider schemes,
    IUserConfirmation<IdentityUser> confirmation)
    : SignInManager<IdentityUser>(users, accessor, claims, options, logger, schemes, confirmation)
{
    public override async Task<SignInResult> PasskeySignInAsync(string credentialJson)
    {
        PasskeyAssertionResult<IdentityUser> verified = await PerformPasskeyAssertionAsync(credentialJson);
        if (!verified.Succeeded) { return SignInResult.Failed; }
        SignInResult? refused = await PreSignInCheck(verified.User);
        if (refused is not null) { return refused; }
        IdentityResult saved = await UserManager.AddOrUpdatePasskeyAsync(verified.User, verified.Passkey);
        return saved.Succeeded
            ? await SignInOrTwoFactorAsync(verified.User, isPersistent: false, bypassTwoFactor: true)
            : SignInResult.Failed;
    }
}
// endsnippet:complete
