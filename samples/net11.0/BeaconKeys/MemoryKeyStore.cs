// snippet:complete
using Microsoft.AspNetCore.Identity;

namespace BeaconKeys;

// One-account execution fixture. No database, multi-user concurrency or production persistence claim.
public sealed class MemoryKeyStore : IUserPasskeyStore<IdentityUser>, IUserLockoutStore<IdentityUser>, IUserEmailStore<IdentityUser>, IUserPhoneNumberStore<IdentityUser>
{
    public IdentityUser Keeper { get; } = new("keeper") { Id = "beacon-keeper", Email = "keeper@example.invalid", LockoutEnabled = true };
    private readonly List<UserPasskeyInfo> keys = [];
    private bool Owns(IdentityUser user) => user.Id == Keeper.Id;
    public Task AddOrUpdatePasskeyAsync(IdentityUser user, UserPasskeyInfo passkey, CancellationToken cancellationToken)
    {
        if (!Owns(user)) { throw new InvalidOperationException("Unknown owner."); }
        keys.RemoveAll(item => item.CredentialId.SequenceEqual(passkey.CredentialId)); keys.Add(passkey); return Task.CompletedTask;
    }
    public Task<IList<UserPasskeyInfo>> GetPasskeysAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult<IList<UserPasskeyInfo>>(Owns(user) ? keys.ToList() : []);
    public Task<IdentityUser?> FindByPasskeyIdAsync(byte[] credentialId, CancellationToken cancellationToken) => Task.FromResult(keys.Any(item => item.CredentialId.SequenceEqual(credentialId)) ? Keeper : null);
    public Task<UserPasskeyInfo?> FindPasskeyAsync(IdentityUser user, byte[] credentialId, CancellationToken cancellationToken) => Task.FromResult(Owns(user) ? keys.SingleOrDefault(item => item.CredentialId.SequenceEqual(credentialId)) : null);
    public Task RemovePasskeyAsync(IdentityUser user, byte[] credentialId, CancellationToken cancellationToken) { if (Owns(user)) { keys.RemoveAll(item => item.CredentialId.SequenceEqual(credentialId)); } return Task.CompletedTask; }
    public Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
    public Task<IdentityResult> UpdateAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
    public Task<IdentityResult> DeleteAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
    public Task<IdentityUser?> FindByIdAsync(string userId, CancellationToken cancellationToken) => Task.FromResult(userId == Keeper.Id ? Keeper : null);
    public Task<IdentityUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) => Task.FromResult(normalizedUserName == Keeper.NormalizedUserName ? Keeper : null);
    public Task<string> GetUserIdAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.Id);
    public Task<string?> GetUserNameAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.UserName);
    public Task SetUserNameAsync(IdentityUser user, string? userName, CancellationToken cancellationToken) { user.UserName = userName; return Task.CompletedTask; }
    public Task<string?> GetNormalizedUserNameAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.NormalizedUserName);
    public Task SetNormalizedUserNameAsync(IdentityUser user, string? normalizedName, CancellationToken cancellationToken) { user.NormalizedUserName = normalizedName; return Task.CompletedTask; }
    public Task<DateTimeOffset?> GetLockoutEndDateAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.LockoutEnd);
    public Task SetLockoutEndDateAsync(IdentityUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken) { user.LockoutEnd = lockoutEnd; return Task.CompletedTask; }
    public Task<int> IncrementAccessFailedCountAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(++user.AccessFailedCount);
    public Task ResetAccessFailedCountAsync(IdentityUser user, CancellationToken cancellationToken) { user.AccessFailedCount = 0; return Task.CompletedTask; }
    public Task<int> GetAccessFailedCountAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.AccessFailedCount);
    public Task<bool> GetLockoutEnabledAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.LockoutEnabled);
    public Task SetLockoutEnabledAsync(IdentityUser user, bool enabled, CancellationToken cancellationToken) { user.LockoutEnabled = enabled; return Task.CompletedTask; }
    public Task SetEmailAsync(IdentityUser user, string? email, CancellationToken cancellationToken) { user.Email = email; return Task.CompletedTask; }
    public Task<string?> GetEmailAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.Email);
    public Task<bool> GetEmailConfirmedAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.EmailConfirmed);
    public Task SetEmailConfirmedAsync(IdentityUser user, bool confirmed, CancellationToken cancellationToken) { user.EmailConfirmed = confirmed; return Task.CompletedTask; }
    public Task<IdentityUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) => Task.FromResult(normalizedEmail == Keeper.NormalizedEmail ? Keeper : null);
    public Task<string?> GetNormalizedEmailAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.NormalizedEmail);
    public Task SetNormalizedEmailAsync(IdentityUser user, string? normalizedEmail, CancellationToken cancellationToken) { user.NormalizedEmail = normalizedEmail; return Task.CompletedTask; }
    public Task SetPhoneNumberAsync(IdentityUser user, string? phoneNumber, CancellationToken cancellationToken) { user.PhoneNumber = phoneNumber; return Task.CompletedTask; }
    public Task<string?> GetPhoneNumberAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.PhoneNumber);
    public Task<bool> GetPhoneNumberConfirmedAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.PhoneNumberConfirmed);
    public Task SetPhoneNumberConfirmedAsync(IdentityUser user, bool confirmed, CancellationToken cancellationToken) { user.PhoneNumberConfirmed = confirmed; return Task.CompletedTask; }
    public void Dispose() { }
}
// endsnippet:complete
