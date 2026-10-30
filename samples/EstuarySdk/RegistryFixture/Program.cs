// snippet:fixture
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

string source = args[0], work = args[1], backend = args[2], major = args[3];
Directory.CreateDirectory(work);
string project = Path.Combine(work, "app");
Directory.CreateDirectory(project);
foreach (string file in new[] { "EstuaryRegistry.csproj", "Program.cs", "global.json" })
    File.Copy(Path.Combine(source, file), Path.Combine(project, file), true);
await File.WriteAllTextAsync(Path.Combine(work, "NuGet.Config"), "<configuration><packageSources><clear/></packageSources></configuration>");
string username = Guid.NewGuid().ToString("N"), password = Guid.NewGuid().ToString("N"), token = Guid.NewGuid().ToString("N");
string basic = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
var transcript = new List<string>();
string realm = "https://auth.estuary.test:5443/token";
int tokens = 0, writes = 0;
using RSA rootKey = RSA.Create(2048), serverKey = RSA.Create(2048);
var rootRequest = new CertificateRequest("CN=Estuary disposable fixture root", rootKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
rootRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
rootRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign, true));
using var root = rootRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddDays(1));
var serverRequest = new CertificateRequest("CN=estuary.test", serverKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
var names = new SubjectAlternativeNameBuilder();
foreach (string name in new[] { "estuary.test", "auth.estuary.test", "localhost" }) names.AddDnsName(name);
names.AddIpAddress(IPAddress.Loopback);
serverRequest.CertificateExtensions.Add(names.Build());
serverRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
serverRequest.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new System.Security.Cryptography.OidCollection { new("1.3.6.1.5.5.7.3.1") }, true));
using var issued = serverRequest.Create(root, DateTimeOffset.UtcNow.AddMinutes(-4), DateTimeOffset.UtcNow.AddHours(12), RandomNumberGenerator.GetBytes(16));
using var certificate = issued.CopyWithPrivateKey(serverKey);
string trust = Path.Combine(work, "roots.pem");
await File.WriteAllTextAsync(trust, await File.ReadAllTextAsync("/etc/ssl/certs/ca-certificates.crt") + "\n" + root.ExportCertificatePem());

var builder = WebApplication.CreateBuilder();
builder.Logging.ClearProviders();
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Loopback, 5080);
    options.Listen(IPAddress.Loopback, 5443, endpoint => endpoint.UseHttps(certificate));
    options.Limits.MaxRequestBodySize = 512 * 1024 * 1024;
});
await using var app = builder.Build();
using var proxy = new HttpClient(new SocketsHttpHandler { AllowAutoRedirect = false, UseProxy = false });
app.Run(async context =>
{
    if (context.Request.Path == "/token")
    {
        if (context.Request.Headers.Authorization.ToString() != basic) { context.Response.StatusCode = 403; return; }
        Interlocked.Increment(ref tokens);
        lock (transcript) transcript.Add($"TOKEN {context.Request.Method} {context.Request.Scheme}://{context.Request.Host}/token; Authorization=Basic [redacted]");
        await context.Response.WriteAsJsonAsync(new { token, expires_in = 300 });
        return;
    }
    if (context.Request.Headers.Authorization.ToString() != "Bearer " + token)
    {
        context.Response.StatusCode = 401;
        context.Response.Headers.WWWAuthenticate = $"Bearer realm=\"{realm}\",service=\"estuary-registry\",scope=\"repository:estuary/cargo:pull,push\"";
        lock (transcript) transcript.Add($"401 {context.Request.Method} {context.Request.Path}; realm={realm}; Authorization=[redacted]");
        return;
    }
    using var request = new HttpRequestMessage(new HttpMethod(context.Request.Method), backend + context.Request.Path + context.Request.QueryString);
    if (context.Request.ContentLength is > 0 || context.Request.Headers.TransferEncoding.Count > 0)
    {
        request.Content = new StreamContent(context.Request.Body);
        if (context.Request.ContentType is { } contentType) request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
    }
    foreach (var header in context.Request.Headers)
        if (header.Key is not ("Host" or "Authorization" or "Content-Length" or "Content-Type" or "Transfer-Encoding"))
            request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
    using var response = await proxy.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);
    context.Response.StatusCode = (int)response.StatusCode;
    foreach (var header in response.Headers.Concat(response.Content.Headers))
        if (header.Key is not ("Transfer-Encoding" or "Connection" or "Location")) context.Response.Headers[header.Key] = header.Value.ToArray();
    if (response.Headers.Location is { } location)
    {
        string relative = location.IsAbsoluteUri ? location.PathAndQuery : location.ToString();
        context.Response.Headers.Location = $"{context.Request.Scheme}://{context.Request.Host}{relative}";
    }
    if (context.Request.Method == "PUT" && context.Request.Path.ToString().StartsWith("/v2/estuary/cargo/manifests/", StringComparison.Ordinal)
        && response.IsSuccessStatusCode) Interlocked.Increment(ref writes);
    lock (transcript) transcript.Add($"REGISTRY {context.Request.Method} {context.Request.Path} => {(int)response.StatusCode}; Authorization=Bearer [redacted]");
    await response.Content.CopyToAsync(context.Response.Body, context.RequestAborted);
});
await app.StartAsync();

async Task<(int Exit, string Output)> Run(string[] arguments, bool credentials = false, bool insecure = false)
{
    var start = new ProcessStartInfo("/usr/share/dotnet/dotnet")
    { WorkingDirectory = project, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
    foreach (string argument in arguments) start.ArgumentList.Add(argument);
    start.Environment["SSL_CERT_FILE"] = trust;
    start.Environment["DOTNET_ADD_GLOBAL_TOOLS_TO_PATH"] = "0";
    start.Environment["DOTNET_CLI_USE_MSBUILD_SERVER"] = "false";
    start.Environment["DOTNET_CLI_HOME"] = Path.Combine(work, "home");
    start.Environment["DOCKER_CONFIG"] = Path.Combine(work, "empty-docker-config");
    start.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
    foreach (string key in new[] { "HTTP_PROXY", "HTTPS_PROXY", "ALL_PROXY" }) start.Environment.Remove(key);
    if (credentials)
    {
        start.Environment["DOTNET_CONTAINER_REGISTRY_UNAME"] = username;
        start.Environment["DOTNET_CONTAINER_REGISTRY_PWORD"] = password;
    }
    if (insecure) start.Environment["DOTNET_CONTAINER_INSECURE_REGISTRIES"] = "127.0.0.1:5080";
    else start.Environment.Remove("DOTNET_CONTAINER_INSECURE_REGISTRIES");
    using var child = Process.Start(start)!;
    Task<string> stdout = child.StandardOutput.ReadToEndAsync(), stderr = child.StandardError.ReadToEndAsync();
    using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
    try { await child.WaitForExitAsync(timeout.Token); }
    catch { child.Kill(true); throw; }
    string output = await stdout + await stderr;
    if (new[] { username, password, token, basic }.Any(secret => output.Contains(secret, StringComparison.Ordinal)))
        throw new InvalidOperationException("Child printed a fixture credential; output withheld");
    transcript.Add($"COMMAND /usr/share/dotnet/dotnet {string.Join(' ', arguments)}; exit={child.ExitCode}\n{output}");
    return (child.ExitCode, output);
}

try
{
    foreach (string[] command in new[] { new[] { "--info" }, new[] { "restore" }, new[] { "build", "-c", "Release", "--no-restore" }, new[] { $"bin/Release/net{major}.0/EstuaryRegistry.dll" } })
    {
        var result = await Run(command);
        if (result.Exit != 0) throw new InvalidOperationException(result.Output);
    }
    var cases = new List<(string Name, string Realm, bool Insecure, string? Error)>
    {
        ("https", "https://auth.estuary.test:5443/token", false, null),
        ("local-http", "http://127.0.0.1:5080/token", true, null)
    };
    if (major == "11") cases.AddRange(new (string Name, string Realm, bool Insecure, string? Error)[]
    {
        ("relative", "token", false, "absolute"),
        ("scheme", "ftp://auth.estuary.test:5443/token", false, "scheme"),
        ("http-secure", "http://auth.estuary.test:5443/token", false, "scheme"),
        ("private", "https://10.23.0.1:5443/token", false, "IP"),
        ("loopback", "https://127.0.0.1:5443/token", false, "IP"),
        ("localhost", "https://localhost:5443/token", false, "IP"),
        ("mismatch", "http://127.0.0.2:5080/token", true, "IP")
    });
    foreach (var test in cases)
    {
        realm = test.Realm;
        int tokensBefore = tokens, writesBefore = writes;
        string registry = test.Insecure ? "127.0.0.1:5080" : "estuary.test:5443";
        var result = await Run(new[] { "publish", "-c", "Release", "--no-restore", "-t:PublishContainer",
            $"-p:ContainerBaseImage={registry}/estuary/base:{major}", $"-p:ContainerRegistry={registry}", $"-p:ContainerImageTag={test.Name}" }, true, test.Insecure);
        if (test.Error is null)
        {
            if (result.Exit != 0 || tokens == tokensBefore || writes == writesBefore) throw new InvalidOperationException($"No real token/push for {test.Name}: {result.Output}");
        }
        else if (result.Exit == 0 || tokens != tokensBefore || writes != writesBefore || !result.Output.Contains(test.Error, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Expected refusal before credentials/token/push: {test.Name}: {result.Output}");
        Console.WriteLine($"PASS net{major} {test.Name}; exit={result.Exit}; tokenRequests={tokens - tokensBefore}; manifestWrites={writes - writesBefore}");
    }
    string sdk = major == "10" ? "10.0.402" : "11.0.100-rc.2.26504.105";
    string runtime = major == "10" ? "10.0.13" : "11.0.0-rc.2.26504.105";
    foreach (string relative in new[] { $"sdk/{sdk}/dotnet.dll", $"sdk/{sdk}/MSBuild.dll",
        $"sdk/{sdk}/Containers/tasks/net{major}.0/Microsoft.NET.Build.Containers.dll", $"sdk/{sdk}/Roslyn/bincore/csc.dll",
        $"packs/Microsoft.NETCore.App.Ref/{runtime}/ref/net{major}.0/System.Runtime.dll",
        $"shared/Microsoft.NETCore.App/{runtime}/libcoreclr.so", "dotnet" })
    {
        string path = $"/usr/share/dotnet/{relative}";
        if (File.Exists(path)) transcript.Add($"ARTIFACT {path}; sha256={Convert.ToHexStringLower(SHA256.HashData(await File.ReadAllBytesAsync(path)))}; product={FileVersionInfo.GetVersionInfo(path).ProductVersion}");
        else throw new FileNotFoundException("Required actual SDK asset", path);
    }
    await File.WriteAllTextAsync(Path.Combine(work, "transcript.txt"), string.Join('\n', transcript));
    Console.WriteLine("PASS: real TLS and local-insecure SDK image publication; redacted HTTP exchanges saved");
}
finally { await app.StopAsync(); }
// endsnippet:fixture
