// snippet:complete
using Microsoft.AspNetCore.Identity;

namespace BeaconKeys;

public static class PasskeyLabels
{
    // Fictional fleet identifier, for the example only. Never identify a device by its name.
    public static readonly Guid FleetAuthenticator = new("32aa82d8-9ea2-42bd-aabc-52e970752036");

    public static string? Suggest(UserPasskeyInfo passkey)
    {
        if (passkey.Aaguid is not { Length: 16 } bytes) { return null; }
        Guid id = new(bytes, bigEndian: true);
        return id == FleetAuthenticator ? "Beacon loan key" : null;
    }

    public static async Task<string> SaveAsync(UserManager<IdentityUser> users, IdentityUser owner, UserPasskeyInfo verifiedPasskey)
    {
        verifiedPasskey.Name ??= Suggest(verifiedPasskey);
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
