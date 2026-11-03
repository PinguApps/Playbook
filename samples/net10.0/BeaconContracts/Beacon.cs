using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Playbook.Samples;

// snippet:contract
[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("9ED59B05-B784-421A-988E-D04A24D11703")]
internal partial interface IBeacon
{
    int GetReading();
    void SetReading(int value);
    string GetCaption();
    void SetCaption(string value);
}

[GeneratedComClass]
internal partial class Beacon : IBeacon
{
    internal int CurrentReading { get; set; } = 12;
    internal string CurrentCaption { get; set; } = "Mere Beacon";
    public int GetReading() => CurrentReading;
    public void SetReading(int value) => CurrentReading = value;
    public string GetCaption() => CurrentCaption;
    public void SetCaption(string value) => CurrentCaption = value;
}
// endsnippet:contract


