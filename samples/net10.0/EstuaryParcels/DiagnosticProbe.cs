// snippet:complete
#if NULL_PROBE
using Microsoft.EntityFrameworkCore.Infrastructure;
static class NullControl
{
    public static void Reject(CosmosDbContextOptionsBuilder builder) => builder.ContentResponseOnWriteEnabled(null);
}
#endif
// endsnippet:complete
