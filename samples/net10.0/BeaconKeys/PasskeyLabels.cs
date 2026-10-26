// snippet:complete
using Microsoft.AspNetCore.Identity;

namespace BeaconKeys;

public static class PasskeyLabels
{
    // .NET 10 has no stored AAGUID on UserPasskeyInfo. Ask the owner for a label.
    public static async Task<string> SaveAsync(UserManager<IdentityUser> users, IdentityUser owner, UserPasskeyInfo verifiedPasskey)
    {
        IdentityResult saved = await users.AddOrUpdatePasskeyAsync(owner, verifiedPasskey);
        if (!saved.Succeeded) { throw new InvalidOperationException("Passkey storage failed."); }
        return string.IsNullOrWhiteSpace(verifiedPasskey.Name)
            ? $"/keys/rename/{System.Buffers.Text.Base64Url.EncodeToString(verifiedPasskey.CredentialId)}"
            : "/keys";
    }

    public static async Task<bool> RenameAsync(UserManager<IdentityUser> users, IdentityUser owner, byte[] credentialId, string name)
    {
        string trimmed = name.Trim();
        if (trimmed.Length is < 1 or > 200) { return false; }
        // Resolve inside the authenticated owner's collection, never globally by credential ID.
        UserPasskeyInfo? passkey = await users.GetPasskeyAsync(owner, credentialId);
        if (passkey is null) { return false; }
        passkey.Name = trimmed;
        return (await users.AddOrUpdatePasskeyAsync(owner, passkey)).Succeeded;
    }
}
// endsnippet:complete
