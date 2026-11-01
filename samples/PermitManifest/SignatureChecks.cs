using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace Playbook.Samples;

internal static class SignatureChecks
{
    public static void Run(string[] arguments)
    {
        if (arguments.Length == 0)
        {
            foreach (string mode in new[] { "default", "strict", "late", "invalid" })
            {
                var start = new ProcessStartInfo(Environment.ProcessPath!) { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
                if (string.Equals(Path.GetFileNameWithoutExtension(Environment.ProcessPath), "dotnet", StringComparison.OrdinalIgnoreCase))
                    start.ArgumentList.Add(Assembly.GetExecutingAssembly().Location);
                start.ArgumentList.Add(mode);
                using var child = Process.Start(start)!;
                Task<string> stdout = child.StandardOutput.ReadToEndAsync();
                Task<string> stderr = child.StandardError.ReadToEndAsync();
                if (!child.WaitForExit(60_000)) { child.Kill(entireProcessTree: true); throw new TimeoutException(mode); }
                Console.Write(stdout.GetAwaiter().GetResult());
                string error = stderr.GetAwaiter().GetResult();
                if (child.ExitCode != 0) throw new InvalidOperationException($"Child {mode}: {error}");
            }
            return;
        }

        string scenario = arguments.Single();
#if NET11_0
        if (scenario == "strict") PermitManifest.ConfigureAtStartup();
#else
        // Prove that the new configuration key does not lower the old package's fixed parser limit.
        if (scenario == "strict") AppContext.SetData("System.Security.Cryptography.MaxReferencesPerSignedInfo", 8);
#endif
        if (scenario == "invalid") AppContext.SetData("System.Security.Cryptography.MaxReferencesPerSignedInfo", "invalid-number");
        using RSA key = RSA.Create(2048);
        var fixtures = new Dictionary<int, XmlDocument>();
        foreach (int count in new[] { 1, 8, 9, 100, 101 }) fixtures.Add(count, Sign(count, key));
        int limit = scenario == "strict" && Environment.Version.Major == 11 ? 8 : 100;
        foreach (int count in fixtures.Keys)
        {
            XmlDocument document = fixtures[count];
            XmlElement signature = Signature(document);
            bool accepted = count <= limit;
            ExpectParse(() => new SignedInfo().LoadXml((XmlElement)signature.FirstChild!), accepted);
            ExpectParse(() => { var verifier = new SignedXml(document); verifier.LoadXml(signature); Require(verifier.CheckSignature(key), "Valid fixture failed signature check."); }, accepted);
            Console.WriteLine($"XML {scenario} count={count} limit={limit} parse-and-verify={(accepted ? "accepted" : "rejected")}");
        }

        XmlDocument valid = fixtures[8];
#if NET11_0
        Require(PermitManifest.Verify(valid, key), "Canonical verification failed.");
        if (scenario == "strict") Throws<CryptographicException>(() => PermitManifest.Verify(fixtures[9], key));
#else
        Require(PermitManifest.Verify(valid, key), "Canonical verification failed.");
        Throws<CryptographicException>(() => PermitManifest.Verify(fixtures[9], key));
        Throws<ArgumentOutOfRangeException>(() => PermitManifest.Verify(valid, key, 0));
#endif
        var malformed = (XmlDocument)valid.CloneNode(true);
        var malformedInfo = (XmlElement)Signature(malformed).FirstChild!;
        malformedInfo.AppendChild(malformed.CreateElement("Unexpected", SignedXml.XmlDsigNamespaceUrl));
        Throws<CryptographicException>(() => new SignedInfo().LoadXml(malformedInfo));
        var tampered = (XmlDocument)valid.CloneNode(true);
        tampered.DocumentElement!.FirstChild!.InnerText = "Altered Merewick permit";
        var check = new SignedXml(tampered); check.LoadXml(Signature(tampered));
        Require(!check.CheckSignature(key), "Tampered content accepted.");
        using RSA wrong = RSA.Create(2048);
        var wrongCheck = new SignedXml(valid); wrongCheck.LoadXml(Signature(valid));
        Require(!wrongCheck.CheckSignature(wrong), "Wrong trusted key accepted.");

        if (scenario == "late")
        {
            AppContext.SetData("System.Security.Cryptography.MaxReferencesPerSignedInfo", 8);
            var cached = new SignedXml(fixtures[9]); cached.LoadXml(Signature(fixtures[9]));
            Require(cached.CheckSignature(key), "Late setting unexpectedly changed cached parser limit.");
        }
        Console.WriteLine($"XML {scenario} malformed/tamper/wrong-key rejected; valid signatures unaffected; cached configuration checked");
    }

    private static XmlDocument Sign(int count, RSA key)
    {
        var document = new XmlDocument { PreserveWhitespace = true, XmlResolver = null };
        document.LoadXml("<Permits><Permit Id='merewick-48'>Merewick survey access</Permit></Permits>");
        var signer = new SignedXml(document) { SigningKey = key };
        signer.SignedInfo!.CanonicalizationMethod = SignedXml.XmlDsigC14NTransformUrl;
        signer.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA256Url;
        for (int i = 0; i < count; i++) signer.AddReference(new Reference("#merewick-48") { DigestMethod = SignedXml.XmlDsigSHA256Url });
        signer.ComputeSignature();
        document.DocumentElement!.AppendChild(document.ImportNode(signer.GetXml(), true));
        Console.WriteLine($"FIXTURE references={count} signature=RSA2048-SHA256 digest=SHA256 uri=#merewick-48 xmlSha256={Convert.ToHexStringLower(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(document.OuterXml)))}");
        return document;
    }

    private static XmlElement Signature(XmlDocument document) => (XmlElement)document.GetElementsByTagName("Signature", SignedXml.XmlDsigNamespaceUrl)[0]!;
    private static void ExpectParse(Action action, bool accepted) { if (accepted) action(); else Throws<CryptographicException>(action); }
    private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
    private static void Throws<T>(Action action) where T : Exception
    {
        try { action(); } catch (T) { return; }
        throw new InvalidOperationException($"Expected {typeof(T).Name}");
    }
}
