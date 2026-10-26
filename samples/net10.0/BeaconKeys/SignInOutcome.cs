// snippet:complete
using Microsoft.AspNetCore.Identity;

namespace BeaconKeys;

public static class SignInOutcome
{
    public static string Message(SignInResult result) => result switch
    {
        { Succeeded: true } => "You are signed in to Beacon stores.",
        { IsLockedOut: true } => "Sign-in is temporarily locked. Try again later or use account recovery.",
        { IsNotAllowed: true } => "Complete the required account confirmation before signing in.",
        _ => "The passkey could not sign you in. Try again or use another sign-in method."
    };
}
// endsnippet:complete
