using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Playbook.Samples;

// snippet:contract
[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("9ED59B05-B784-421A-988E-D04A24D11703")]
internal partial interface IBeacon
{
    int Reading { get; set; }
    string Caption { get; set; }
}

[GeneratedComClass]
internal partial class Beacon : IBeacon
{
    internal int CurrentReading { get; set; } = 12;
    public int Reading { get => CurrentReading; set => CurrentReading = value; }
    internal string CurrentCaption { get; set; } = "Mere Beacon";
    public string Caption { get => CurrentCaption; set => CurrentCaption = value; }
}
// endsnippet:contract
