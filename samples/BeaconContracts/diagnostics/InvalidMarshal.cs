using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
namespace Playbook.Samples;
[GeneratedComInterface]
[Guid("DE620CE1-52A5-42BA-AB18-78FCA30F497F")]
internal partial interface IInvalidMarshal
{
    int Value { [return: MarshalUsing(CountElementName = "length")] get; }
}
