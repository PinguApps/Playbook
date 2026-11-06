// snippet:complete
namespace BreakwaterNames;

public static class NameReader
{
    public static string Read()
    {
        using Java.Lang.Class type = Java.Lang.Class.ForName("org.breakwater.names.Beacon");
        using Java.Lang.Reflect.Constructor constructor = type.GetDeclaredConstructor();
        using Java.Lang.Object instance = constructor.NewInstance()!;
        using Java.Lang.Reflect.Method method = type.GetDeclaredMethod("signal");
        method.Accessible = true;
        using Java.Lang.Object result = method.Invoke(instance)!;
        return result.ToString()!;
    }
}
// endsnippet:complete