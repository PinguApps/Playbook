// snippet:complete
namespace SandpiperPlatformAndroid;
[TestClass]
public sealed class CacheDirectoryTests
{
    [TestMethod]
    public void SurveyCheckpointDirectoryIsAbsolute()
    {
        var directory = Android.App.Application.Context.CacheDir!.AbsolutePath;
        Assert.IsTrue(Path.IsPathFullyQualified(directory));
    }
}
// endsnippet:complete
