using System.Runtime.InteropServices;
namespace Playbook.Samples;
internal static partial class Safety
{
    [LibraryImport("unused-probe-library")]
    internal static partial int Read();
}
