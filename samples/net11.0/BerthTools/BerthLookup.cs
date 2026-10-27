// snippet:complete
using System.ComponentModel;
using ModelContextProtocol.Server;

namespace BerthTools;

[McpServerToolType]
public sealed class BerthLookup
{
    [McpServerTool, Description("Look up the load limit for a fictional harbour berth.")]
    public static string LoadLimit([Description("Berth code, for example W3.")] string berth) =>
        berth == "W3" ? "W3 permits 18 tonnes." : "No load limit is registered for that berth.";
}
// endsnippet:complete
