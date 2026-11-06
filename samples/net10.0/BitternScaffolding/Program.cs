// snippet:complete
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Sqlite.Design.Internal;
using Microsoft.Extensions.DependencyInjection;
var output = Path.GetFullPath(args.Single());
Directory.CreateDirectory(output);
var connection = "Data Source=" + Path.Combine(output, "bittern.db");
using (var db = new SqliteConnection(connection)) {
    db.Open(); using var command = db.CreateCommand();
    command.CommandText = "CREATE TABLE Observations (Id INTEGER PRIMARY KEY, Station TEXT NOT NULL, Depth REAL NULL); INSERT INTO Observations VALUES (1, 'Reedbank', 12.5), (2, 'Mudflat', NULL);";
    command.ExecuteNonQuery();
}
var services = new ServiceCollection();
services.AddEntityFrameworkDesignTimeServices();
#pragma warning disable EF1001 // Required provider design-time registration, pinned and verified.
new SqliteDesignTimeServices().ConfigureDesignTimeServices(services);
#pragma warning restore EF1001
using var provider = services.BuildServiceProvider();
var selector = provider.GetRequiredService<IModelCodeGeneratorSelector>();
var options = new ModelCodeGenerationOptions { Language = "C#", ContextName = "MarshContext", ModelNamespace = "Bittern.Generated", ContextNamespace = "Bittern.Generated", ConnectionString = connection, SuppressOnConfiguring = true, UseNullableReferenceTypes = true };
var extension = new BitternSelector(selector);
var generator = extension.Select(options);
if (generator.Language != "C#") throw new Exception("Wrong generator");
services.AddSingleton<IModelCodeGeneratorSelector>(extension);
using var decorated = services.BuildServiceProvider();
var scaffolder = decorated.GetRequiredService<IReverseEngineerScaffolder>();
var model = scaffolder.ScaffoldModel(connection, new DatabaseModelFactoryOptions(), new ModelReverseEngineerOptions(), options);
scaffolder.Save(model, output, true);
if (extension.Calls < 2) throw new Exception("Scaffolder bypassed extension");
Console.WriteLine("PASS selector receives explicit options and scaffolds SQLite schema: " + output);
sealed class BitternSelector(IModelCodeGeneratorSelector inner) : IModelCodeGeneratorSelector {
    public int Calls { get; private set; }
#pragma warning disable CS0618 // Retained old contract control.
    public IModelCodeGenerator Select(string? language) { Calls++; return inner.Select(language); }
#pragma warning restore CS0618
    public IModelCodeGenerator Select(ModelCodeGenerationOptions options) { return Select(options.Language); }
}
// endsnippet:complete