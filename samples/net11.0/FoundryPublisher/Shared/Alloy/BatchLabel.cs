// snippet:alloy-source
namespace AsterFoundry;

public static class BatchLabel
{
    public static string Format(int furnace, int batch) => $"F{furnace:D2}-B{batch:D4}";
}
// endsnippet:alloy-source
