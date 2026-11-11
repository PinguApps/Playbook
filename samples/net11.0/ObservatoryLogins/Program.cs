// snippet:complete
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

// Saltmarsh Observatory never normalizes opaque provider names or keys.
await using var connection = new SqliteConnection("Data Source=:memory:");
await connection.OpenAsync();
connection.CreateCollation("OBSERVATORY", (left, right) =>
    StringComparer.OrdinalIgnoreCase.Compare(left.TrimEnd(' '), right.TrimEnd(' ')));
await using var database = new Observatory(new DbContextOptionsBuilder<Observatory>().UseSqlite(connection).Options);
await database.Database.EnsureCreatedAsync();
using var builtin = new UserStore<IdentityUser>(database);
using var guarded = new ExactLoginStore(database);
var observer = new IdentityUser("observer") { Id = "observer-1" };
var other = new IdentityUser("other") { Id = "observer-2" };
database.Users.AddRange(observer, other);
await database.SaveChangesAsync();
foreach (var (provider, key, exact) in new[] {
    ("TidalAuthority", "Station-A7", true), ("tidalauthority", "Station-A7", false),
    ("TidalAuthority", "station-a7", false), ("TidalAuthority ", "Station-A7", false),
    ("TidalAuthority", "Station-A7 ", false), ("MissingAuthority", "Station-A7", false) })
{
    database.UserLogins.Add(new IdentityUserLogin<string> {
        LoginProvider = "TidalAuthority", ProviderKey = "Station-A7", UserId = observer.Id });
    await database.SaveChangesAsync();
    database.ChangeTracker.Clear();
    var candidateExists = await database.UserLogins.AnyAsync(x => x.LoginProvider == provider && x.ProviderKey == key);
    Check(candidateExists == (provider != "MissingAuthority"), "real SQL collation candidate");
    Console.WriteLine($"OBSERVED built-in lookup [{provider}]/[{key}]: {await builtin.FindByLoginAsync(provider, key) is not null}");
    Check((await guarded.FindByLoginAsync(provider, key) is not null) == exact, "guarded lookup");
    await guarded.RemoveLoginAsync(other, provider, key);
    await database.SaveChangesAsync();
    Check(await database.UserLogins.AnyAsync(), "wrong owner cannot remove login");
    await guarded.RemoveLoginAsync(observer, provider, key);
    await database.SaveChangesAsync();
    Check(await database.UserLogins.AnyAsync() == !exact, "guarded removal");
    await database.UserLogins.ExecuteDeleteAsync();
    database.ChangeTracker.Clear();
    // Independently observe the unguarded removal against a fresh row.
    database.UserLogins.Add(new IdentityUserLogin<string> {
        LoginProvider = "TidalAuthority", ProviderKey = "Station-A7", UserId = observer.Id });
    await database.SaveChangesAsync();
    database.ChangeTracker.Clear();
    await builtin.RemoveLoginAsync(observer, provider, key);
    await database.SaveChangesAsync();
    Console.WriteLine($"OBSERVED built-in removal [{provider}]/[{key}]: removed={!await database.UserLogins.AnyAsync()}");
    await database.UserLogins.ExecuteDeleteAsync();
    database.ChangeTracker.Clear();
    Console.WriteLine($"PASS guarded lookup/removal; provider=[{provider}];key=[{key}];exact={exact}");
}
static void Check(bool condition, string claim) {
    if (!condition) throw new InvalidOperationException(claim);
}
// Register this store in place of the default store when these guards are needed.
// Other Identity operations remain inherited. Keep schema/key length and migrations application-owned.
sealed class ExactLoginStore(Observatory database) : UserStore<IdentityUser>(database)
{
    private async Task<IdentityUserLogin<string>?> ExactLogin(string provider, string key, CancellationToken token) {
        var candidate = await database.UserLogins.SingleOrDefaultAsync(x =>
            x.LoginProvider == provider && x.ProviderKey == key, token);
        return candidate is not null &&
            string.Equals(candidate.LoginProvider, provider, StringComparison.Ordinal) &&
            string.Equals(candidate.ProviderKey, key, StringComparison.Ordinal) ? candidate : null;
    }
    public override async Task<IdentityUser?> FindByLoginAsync(string loginProvider, string providerKey,
        CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        var login = await ExactLogin(loginProvider, providerKey, cancellationToken);
        return login is null ? null : await FindByIdAsync(login.UserId, cancellationToken);
    }
    public override async Task RemoveLoginAsync(IdentityUser user, string loginProvider, string providerKey,
        CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(user);
        cancellationToken.ThrowIfCancellationRequested();
        var login = await ExactLogin(loginProvider, providerKey, cancellationToken);
        if (login is not null && string.Equals(login.UserId, user.Id, StringComparison.Ordinal))
            database.UserLogins.Remove(login);
        // UserManager commits through UpdateAsync; direct store callers must save changes.
    }
}
sealed class Observatory(DbContextOptions<Observatory> options) : IdentityDbContext<IdentityUser>(options)
{
    protected override void OnModelCreating(ModelBuilder builder) {
        base.OnModelCreating(builder);
        builder.Entity<IdentityUserLogin<string>>().Property(x => x.LoginProvider).UseCollation("OBSERVATORY");
        builder.Entity<IdentityUserLogin<string>>().Property(x => x.ProviderKey).UseCollation("OBSERVATORY");
    }
}
// endsnippet:complete
