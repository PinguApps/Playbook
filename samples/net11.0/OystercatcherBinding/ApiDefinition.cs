// snippet:complete
using Foundation;
using ObjCRuntime;

namespace Oystercatcher;

[BaseType (typeof (NSObject))]
[DisableDefaultCtor]
interface SurveyPermit
{
    [Export ("initWithStationCode:")]
    [FactoryMethod ("TryCreate")]
    [return: NullAllowed]
    NativeHandle Constructor (string stationCode);
}
// endsnippet:complete
