using Microsoft.Build.Experimental.FileAccess;

namespace CwmSeedBank.Build;

// snippet:decoder
public sealed record AccessObservation(string Path, string Operation,
    string RequestedAccess, uint Error, string RequestedAttributes,
    string? EnumerationPattern, string OpenedAttributes);

public static class AccessDecoder
{
    public static AccessObservation Read(FileAccessData access) => new(
        access.Path, access.Operation.ToString(), access.RequestedAccess.ToString(),
        access.Error, access.FlagsAndAttributes.ToString(),
        access.EnumeratePattern, access.OpenedFileOrDirectoryAttributes.ToString());
}
// endsnippet:decoder
