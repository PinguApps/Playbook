// snippet:complete
using System.Runtime.InteropServices;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace ReedPermits;

internal static class Program
{
    public static void Main()
    {
        foreach (bool configurationOnly in new[] { false, true })
        {
            foreach (bool sqlite in new[] { false, true })
            {
                CheckReplacement(configurationOnly, sqlite);
            }
        }
        CheckFailures();
        CheckOptionsReplacement();
        Console.WriteLine($"PASS: isolated provider replacement; runtime {RuntimeInformation.FrameworkDescription}; EF {typeof(DbContext).Assembly.GetName().Version}.");
    }

    private static ServiceCollection Original(SqliteConnection sentinel, Action called)
    {
        ServiceCollection services = new();
        // A closed, disposable stand-in. Never load production settings or resolve this registration unchanged.
        services.AddDbContext<PermitContext>(options =>
        {
            called();
            options.UseSqlite(sentinel, provider => provider.CommandTimeout(73));
            options.EnableSensitiveDataLogging();
        }, contextLifetime: ServiceLifetime.Transient, optionsLifetime: ServiceLifetime.Singleton);
        services.AddSingleton(new FixtureMarker());
        return services;
    }

    private static void CheckReplacement(bool configurationOnly, bool sqlite)
    {
        using SqliteConnection sentinel = new("Data Source=:memory:");
        using SqliteConnection testConnection = new("Data Source=:memory:");
        int originalCalls = 0;
        ServiceCollection services = Original(sentinel, () => originalCalls++);
        ServiceDescriptor contextRegistration = services.Single(d => d.ServiceType == typeof(PermitContext));
        ServiceDescriptor optionsRegistration = services.Single(d => d.ServiceType == typeof(DbContextOptions<PermitContext>));

        // snippet:remove-registration
        if (configurationOnly) { services.RemoveDbContext<PermitContext>(removeConfigurationOnly: true); }
        else { services.RemoveDbContext<PermitContext>(); } // Exercise the default false.
        Require(!services.Any(d => d.ServiceType == typeof(IDbContextOptionsConfiguration<PermitContext>)), "callbacks removed");
        if (configurationOnly)
        {
            Require(services.Contains(contextRegistration) && services.Contains(optionsRegistration), "registrations retained");
        }
        else
        {
            Require(!services.Contains(contextRegistration) && !services.Contains(optionsRegistration), "registrations removed");
        }
        // endsnippet:remove-registration

        string databaseName = Guid.NewGuid().ToString("N");
        void Configure(DbContextOptionsBuilder options)
        {
            if (sqlite) { options.UseSqlite(testConnection); }
            else { options.UseInMemoryDatabase(databaseName); }
        }
        if (configurationOnly)
        {
            // Preserve Transient context / Singleton options; configuration must not capture scoped services.
            services.ConfigureDbContext<PermitContext>(Configure, ServiceLifetime.Singleton);
        }
        else
        {
            // Re-register deliberately with the normal Scoped / Scoped defaults.
            services.AddDbContext<PermitContext>(Configure);
        }

        using ServiceProvider provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        using IServiceScope first = provider.CreateScope();
        using IServiceScope second = provider.CreateScope();
        DbContextOptions<PermitContext> options1 = first.ServiceProvider.GetRequiredService<DbContextOptions<PermitContext>>();
        DbContextOptions<PermitContext> options2 = second.ServiceProvider.GetRequiredService<DbContextOptions<PermitContext>>();
        Require(ReferenceEquals(options1, options2) == configurationOnly, "resolved options lifetime");
        PermitContext context1 = first.ServiceProvider.GetRequiredService<PermitContext>();
        PermitContext repeated = first.ServiceProvider.GetRequiredService<PermitContext>();
        Require(ReferenceEquals(context1, repeated) != configurationOnly, "resolved context lifetime");
        Require(!ReferenceEquals(context1, second.ServiceProvider.GetRequiredService<PermitContext>()), "context scope isolation");
        Require(provider.GetRequiredService<FixtureMarker>() is not null, "unrelated service retained");
        Require(originalCalls == 0, "removed callback was never evaluated");
        Require(!options1.GetExtension<CoreOptionsExtension>().IsSensitiveDataLoggingEnabled, "original flags removed");
        AssertProvider(options1, sqlite, testConnection);
        Require(context1.Database.ProviderName == (sqlite ? "Microsoft.EntityFrameworkCore.Sqlite" : "Microsoft.EntityFrameworkCore.InMemory"), "resolved provider");
        if (sqlite) { testConnection.Open(); }
        Crud(provider, sqlite);
        Require(sentinel.State == System.Data.ConnectionState.Closed, "original connection never opened");
        if (sqlite)
        {
            using SqliteCommand version = testConnection.CreateCommand();
            version.CommandText = "SELECT sqlite_version()";
            Console.WriteLine($"SQLite {version.ExecuteScalar()}; native source {SQLitePCL.raw.sqlite3_sourceid().utf8_to_string()}");
        }
        Console.WriteLine($"PASS: configurationOnly={configurationOnly}, sqlite={sqlite}; lifetimes, services/options, CRUD and unique-index boundary.");
    }

    private static void Crud(ServiceProvider provider, bool sqlite)
    {
        using (IServiceScope scope = provider.CreateScope())
        {
            PermitContext context = scope.ServiceProvider.GetRequiredService<PermitContext>();
            context.Database.EnsureCreated();
            context.Permits.Add(new() { Code = "REED-14" });
            Require(context.SaveChanges() == 1, "create");
        }
        using (IServiceScope scope = provider.CreateScope())
        {
            PermitContext context = scope.ServiceProvider.GetRequiredService<PermitContext>();
            Permit permit = context.Permits.Single();
            Require(permit.Code == "REED-14", "read across scopes");
            permit.Code = "REED-15";
            Require(context.SaveChanges() == 1, "update");
        }
        using (IServiceScope scope = provider.CreateScope())
        {
            PermitContext context = scope.ServiceProvider.GetRequiredService<PermitContext>();
            Require(context.Permits.AsNoTracking().Single().Code == "REED-15", "persisted update");
            context.Permits.Add(new() { Code = "REED-15" });
            if (sqlite) { Reject<DbUpdateException>(() => context.SaveChanges(), "UNIQUE constraint failed"); }
            else { Require(context.SaveChanges() == 1, "InMemory does not enforce the relational unique index"); }
        }
        using (IServiceScope scope = provider.CreateScope())
        {
            PermitContext context = scope.ServiceProvider.GetRequiredService<PermitContext>();
            Require(context.Permits.Count() == (sqlite ? 1 : 2), "failed SQLite save did not insert a duplicate");
            context.Permits.RemoveRange(context.Permits);
            context.SaveChanges();
        }
        using (IServiceScope scope = provider.CreateScope())
        {
            Require(!scope.ServiceProvider.GetRequiredService<PermitContext>().Permits.Any(), "persisted delete");
        }
    }

    private static void CheckFailures()
    {
        using SqliteConnection sentinel = new("Data Source=:memory:");
        int calls = 0;
        ServiceCollection mixed = Original(sentinel, () => calls++);
        // Replacing just the typed options descriptor leaves the old callback behind.
        Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions
            .RemoveAll<DbContextOptions<PermitContext>>(mixed);
        mixed.AddDbContext<PermitContext>(o => o.UseInMemoryDatabase("mixed"));
        using ServiceProvider provider = mixed.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();
        DbContextOptions<PermitContext> options = scope.ServiceProvider.GetRequiredService<DbContextOptions<PermitContext>>();
        Require(options.Extensions.Count(e => e.Info.IsDatabaseProvider) == 2 && calls == 1, "retained callback reproduced");
        using PermitContext context = new(options);
        Reject<InvalidOperationException>(() => _ = context.Model, "Only a single database provider");

        ServiceCollection retained = Original(sentinel, () => { });
        using SqliteConnection testConnection = new("Data Source=:memory:");
        retained.AddDbContext<PermitContext>(o => o.UseSqlite(testConnection));
        using ServiceProvider retainedProvider = retained.BuildServiceProvider();
        DbContextOptions<PermitContext> retainedOptions = retainedProvider.GetRequiredService<DbContextOptions<PermitContext>>();
        Require(retainedOptions.Extensions.Count(e => e.Info.IsDatabaseProvider) == 1, "one provider alone is insufficient");
        Reject<InvalidOperationException>(() => Require(!retainedOptions.GetExtension<CoreOptionsExtension>().IsSensitiveDataLoggingEnabled,
            "retained sensitive logging"), "retained sensitive logging");
        Require(sentinel.State == System.Data.ConnectionState.Closed, "failure controls never opened original connection");
        Console.WriteLine("PASS: retained callback causes mixed-provider failure; same-provider retained sensitive logging rejected before database use.");
    }

    private static void CheckOptionsReplacement()
    {
        using SqliteConnection sentinel = new("Data Source=:memory:");
        DbContextOptions<PermitContext> original = new DbContextOptionsBuilder<PermitContext>()
            .EnableDetailedErrors().UseSqlite(sentinel).Options;
        // snippet:without-extension
        DbContextOptions<PermitContext> stripped = (DbContextOptions<PermitContext>)ProviderOptions.WithoutProvider(original);
        Require(original.Extensions.Any(e => e.Info.IsDatabaseProvider), "original immutable options retained");
        Require(!stripped.Extensions.Any(e => e.Info.IsDatabaseProvider), "copy has no SQLite extension");
        DbContextOptions<PermitContext> memory = new DbContextOptionsBuilder<PermitContext>(stripped)
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N")).Options;
        // endsnippet:without-extension
        Require(memory.GetExtension<CoreOptionsExtension>().DetailedErrorsEnabled, "unrelated core settings preserved");
        AssertProvider(memory, false, sentinel);
        using (PermitContext context = new(memory))
        {
            context.Permits.Add(new() { Code = "COPY-1" });
            context.SaveChanges();
        }
        using (PermitContext context = new(memory)) { Require(context.Permits.Single().Code == "COPY-1", "options-copy execution"); }

        DbContextOptionsBuilder<PermitContext> builder = new(original);
        builder.UseMemoryPermitStore(Guid.NewGuid().ToString("N"));
        builder.UseMemoryPermitStore(Guid.NewGuid().ToString("N")); // Missing extension removal is a no-op.
        AssertProvider(builder.Options, false, sentinel);
        Require(builder.Options.GetExtension<CoreOptionsExtension>().DetailedErrorsEnabled, "builder retains core settings");
        using (PermitContext context = new(builder.Options))
        {
            context.Permits.Add(new() { Code = "BUILDER-1" });
            context.SaveChanges();
            Require(context.Permits.Single().Code == "BUILDER-1", "builder extension execution");
        }
        Require(sentinel.State == System.Data.ConnectionState.Closed, "options controls never opened original connection");
        Console.WriteLine("PASS: WithoutExtension immutable copy and extension-author RemoveExtension/no-op; core settings preserved and actual InMemory writes read.");
    }

    private static void AssertProvider(DbContextOptions<PermitContext> options, bool sqlite, SqliteConnection testConnection)
    {
        Require(options.Extensions.Count(e => e.Info.IsDatabaseProvider) == 1, "exactly one provider");
        RelationalOptionsExtension? extension = options.Extensions.OfType<RelationalOptionsExtension>().SingleOrDefault();
        Require(sqlite ? ReferenceEquals(extension?.Connection, testConnection) : extension is null, "only the selected test connection");
        Console.WriteLine("Options: " + string.Join(", ", options.Extensions.Select(e => e.GetType().Name)));
    }

    private static void Reject<T>(Action action, string fragment) where T : Exception
    {
        try { action(); }
        catch (T exception) when (exception.ToString().Contains(fragment, StringComparison.Ordinal)) { return; }
        throw new InvalidOperationException($"Expected {typeof(T).Name}: {fragment}");
    }

    private static void Require(bool condition, string claim)
    {
        if (!condition) { throw new InvalidOperationException(claim); }
    }
}

// snippet:remove-extension
internal static class PermitOptionsExtensions
{
    // Infrastructure API belongs inside an options extension, not scattered through application setup.
    public static DbContextOptionsBuilder<PermitContext> UseMemoryPermitStore(
        this DbContextOptionsBuilder<PermitContext> builder, string name)
    {
        ProviderOptions.RemoveProvider(builder);
        return builder.UseInMemoryDatabase(name);
    }
}
// endsnippet:remove-extension

internal static class ProviderOptions
{
    // Use only public APIs and the runtime extension type, not a provider's unsupported Internal types.
    // Reflection here targets public generic methods; trimming / NativeAOT is outside this console example.
    public static DbContextOptions WithoutProvider(DbContextOptions options)
    {
        Type extension = options.Extensions.Single(e => e.Info.IsDatabaseProvider).GetType();
        return (DbContextOptions)typeof(DbContextOptions).GetMethod(nameof(DbContextOptions.WithoutExtension))!
            .MakeGenericMethod(extension).Invoke(options, null)!;
    }

    public static void RemoveProvider(DbContextOptionsBuilder builder)
    {
        // Remove SQLite when present; the second call demonstrates an absent exact type is a no-op.
        Type? sqliteExtension = builder.Options.Extensions.SingleOrDefault(e => e.Info.IsDatabaseProvider &&
            e.GetType().Assembly == typeof(SqliteDbContextOptionsBuilderExtensions).Assembly)?.GetType();
        if (sqliteExtension is null) { return; }
        System.Reflection.MethodInfo remove = typeof(IDbContextOptionsBuilderInfrastructure)
            .GetMethod(nameof(IDbContextOptionsBuilderInfrastructure.RemoveExtension))!.MakeGenericMethod(sqliteExtension);
        remove.Invoke(builder, null);
        remove.Invoke(builder, null);
    }
}

internal sealed class PermitContext(DbContextOptions<PermitContext> options) : DbContext(options)
{
    public DbSet<Permit> Permits => Set<Permit>();
    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<Permit>().HasIndex(p => p.Code).IsUnique();
}

internal sealed class Permit
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
}

internal sealed class FixtureMarker;
// endsnippet:complete
