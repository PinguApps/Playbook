// snippet:observer
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Execution;
using Microsoft.Build.Experimental.FileAccess;
using Microsoft.Build.ProjectCache;

namespace CwmSeedBank.Build;

// Observation only: this plugin always misses and never publishes cache entries.
public sealed class SeedBuildObserver : ProjectCachePluginBase
{
    private readonly object gate = new();
    private readonly Dictionary<string, List<string>> pending = new();
    private readonly string root = Environment.GetEnvironmentVariable("SEED_OBSERVER_ROOT")
        ?? throw new InvalidOperationException("Set SEED_OBSERVER_ROOT to the disposable input directory.");
    private readonly string receipt = Environment.GetEnvironmentVariable("SEED_OBSERVER_RECEIPT")
        ?? throw new InvalidOperationException("Set SEED_OBSERVER_RECEIPT to an application-owned output file.");

    public override Task BeginBuildAsync(CacheContext context, PluginLoggerBase logger,
        CancellationToken cancellationToken)
    {
        WriteReceipt("begin\n");
        return Task.CompletedTask;
    }

    public override Task<CacheResult> GetCacheResultAsync(BuildRequestData buildRequest,
        PluginLoggerBase logger, CancellationToken cancellationToken) =>
        Task.FromResult(CacheResult.IndicateNonCacheHit(CacheResultType.CacheMiss));

    public override void HandleFileAccess(FileAccessContext fileAccessContext, FileAccessData fileAccessData)
    {
        if (!string.Equals(fileAccessData.Path, root, StringComparison.OrdinalIgnoreCase)
            && !fileAccessData.Path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            return;
        AccessObservation observation = AccessDecoder.Read(fileAccessData);
        string key = Key(fileAccessContext);
        lock (gate)
        {
            if (!pending.TryGetValue(key, out List<string>? records))
                pending.Add(key, records = new List<string>());
            if (records.Count >= 10000)
                throw new InvalidOperationException("Disposable observer capture limit exceeded.");
            // Do not retain or log ProcessArgs. Escape paths/patterns before writing TSV.
            records.Add(string.Join("\t", "access", key, Escape(observation.Path),
                observation.Operation, observation.RequestedAccess, observation.Error,
                observation.RequestedAttributes, Escape(observation.EnumerationPattern),
                observation.OpenedAttributes));
        }
    }

    public override Task HandleProjectFinishedAsync(FileAccessContext fileAccessContext,
        BuildResult buildResult, PluginLoggerBase logger, CancellationToken cancellationToken)
    {
        lock (gate)
        {
            string key = Key(fileAccessContext);
            Drain(key);
            string status = cancellationToken.IsCancellationRequested ? "cancelled"
                : buildResult.OverallResult == BuildResultCode.Success ? "success" : "failed";
            WriteReceipt($"finished\t{key}\t{status}\n");
        }
        // Invalidate/drain before propagating cancellation, never publish partial state.
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public override Task EndBuildAsync(PluginLoggerBase logger, CancellationToken cancellationToken)
    {
        lock (gate)
        {
            foreach (string key in pending.Keys.ToArray())
            {
                Drain(key);
                WriteReceipt($"unfinished\t{key}\tabandoned\n");
            }
            WriteReceipt("end\n");
        }
        return Task.CompletedTask;
    }

    private void Drain(string key)
    {
        if (!pending.TryGetValue(key, out List<string>? records)) return;
        File.AppendAllLines(receipt, records);
        pending.Remove(key);
    }

    // Serialize the small disposable receipt synchronously across callbacks. The
    // Framework reporting host does not provide File.AppendAllTextAsync.
    private void WriteReceipt(string text) => File.AppendAllText(receipt, text);

#pragma warning disable CA1307 // Framework's ordinal string.Replace has no comparison overload.
    private static string Key(FileAccessContext context)
    {
        // Length-prefix every value and collection; property contents can include
        // newlines or '='. Hashing ambiguous delimiter text would merge contexts.
        using MemoryStream identity = new();
        using (BinaryWriter writer = new(identity, new UTF8Encoding(false, true), true))
        {
            writer.Write(context.ProjectFullPath);
            writer.Write(context.GlobalProperties.Count);
            foreach (KeyValuePair<string, string> property in
                context.GlobalProperties.OrderBy(p => p.Key, StringComparer.Ordinal))
            {
                writer.Write(property.Key);
                writer.Write(property.Value);
            }
            writer.Write(context.Targets.Count);
            foreach (string target in context.Targets) writer.Write(target);
        }
        using SHA256 hash = SHA256.Create();
        return BitConverter.ToString(hash.ComputeHash(identity.ToArray())).Replace("-", "");
    }

    private static string Escape(string? value) => value is null ? "<absent>"
        : value.Replace("\\", "\\\\").Replace("\t", "\\t").Replace("\r", "\\r").Replace("\n", "\\n");
#pragma warning restore CA1307
}
// endsnippet:observer
