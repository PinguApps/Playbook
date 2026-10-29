// snippet:plugin
namespace Tern.Plugin;
public static class Entry
{
    public static string Read() => "west:" + Rules.Capacity.Remaining(20, 7);
}
// endsnippet:plugin
