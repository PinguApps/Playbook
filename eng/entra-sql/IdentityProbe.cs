using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography.X509Certificates;

// Access provisioning only. Recipe consumers must verify their own authentication mode.
using var certificate = X509CertificateLoader.LoadPkcs12FromFile(args[0], "");
var credential = new ClientCertificateCredential(
    "e098693d-4698-42b9-81bb-83881e28a826",
    "836d8bc3-2e01-4e20-908b-4fbfeb1569a2", certificate);
var token = await credential.GetTokenAsync(new TokenRequestContext(["https://database.windows.net/.default"]));
await using var connection = new SqlConnection(
    $"Server=127.0.0.1,{int.Parse(args[1], System.Globalization.CultureInfo.InvariantCulture)};Database=playbook_entra_pin679;Encrypt=True;TrustServerCertificate=True;Connect Timeout=30")
    { AccessToken = token.Token };
await connection.OpenAsync();
await using var command = connection.CreateCommand();
command.CommandText = "SELECT Label FROM dbo.IdentityProbe WHERE Id = 1";
if ((string?)await command.ExecuteScalarAsync() != "Estuary test")
    throw new InvalidOperationException("Unexpected probe row.");
Console.WriteLine("PASS: real Entra certificate token accepted; test row read.");
await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
command.Transaction = transaction;
try
{
    foreach (var sql in new[] { "INSERT dbo.IdentityProbe VALUES (2, N'forbidden')", "USE msdb; SELECT COUNT(*) FROM dbo.sysjobs" })
    {
#pragma warning disable CA2100 // Fixed literal denial probes, no external SQL input.
        command.CommandText = sql;
#pragma warning restore CA2100
        try
        {
            await command.ExecuteScalarAsync();
            throw new InvalidOperationException("Isolation failure: forbidden operation succeeded.");
        }
        catch (SqlException error) when (error.Number is 229 or 916)
        {
            Console.WriteLine($"PASS: forbidden operation denied ({error.Number}).");
        }
    }
}
finally
{
    await transaction.RollbackAsync();
}
