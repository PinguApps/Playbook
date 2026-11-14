// snippet:complete
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Execution;
using Microsoft.Build.Experimental.FileAccess;
using Microsoft.Build.ProjectCache;
using CwmSeedBank.Build;

namespace CwmSeedBank.Host
{
    // Compile the unchanged older decoder with only its namespace renamed to
    // CwmSeedBank.Previous. Compare both decoders on every REAL host callback.
    public sealed class ObserverProbe : ProjectCachePluginBase
    {
        private readonly SeedBuildObserver observer = new SeedBuildObserver();
        public override Task BeginBuildAsync(CacheContext context, PluginLoggerBase logger, CancellationToken token)
        {
            // Synthetic construction qualifies defaults only, never instrumentation.
            var oldOverload = new FileAccessData(default, default, 0, 0, 0, 0, 0, 0, "unused", "", false);
            if (oldOverload.EnumeratePattern != null || oldOverload.OpenedFileOrDirectoryAttributes != 0)
                throw new InvalidOperationException("Old constructor defaults changed.");
            return observer.BeginBuildAsync(context, logger, token);
        }
        public override Task<CacheResult> GetCacheResultAsync(BuildRequestData request, PluginLoggerBase logger,
            CancellationToken token) => observer.GetCacheResultAsync(request, logger, token);
        public override void HandleFileAccess(FileAccessContext context, FileAccessData access)
        {
            var previous = Previous.AccessDecoder.Read(access);
            var current = AccessDecoder.Read(access);
            if (previous.Path != current.Path || previous.Operation != current.Operation
                || previous.RequestedAccess != current.RequestedAccess || previous.Error != current.Error
                || previous.RequestedAttributes != current.RequestedAttributes)
                throw new InvalidOperationException("Previous/current decoding disagreed.");
            observer.HandleFileAccess(context, access);
        }
        public override Task HandleProjectFinishedAsync(FileAccessContext context, BuildResult result,
            PluginLoggerBase logger, CancellationToken token) => observer.HandleProjectFinishedAsync(context, result, logger, token);
        public override Task EndBuildAsync(PluginLoggerBase logger, CancellationToken token) => observer.EndBuildAsync(logger, token);
    }
}
// endsnippet:complete
