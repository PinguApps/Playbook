// snippet:complete
namespace SandpiperPlatformIos;
[TestClass]
public sealed class CacheDirectoryTests
{
    [TestMethod]
    public void SurveyCheckpointDirectoryIsAbsolute()
    {
        var directory = Foundation.NSFileManager.DefaultManager.GetUrls(Foundation.NSSearchPathDirectory.CachesDirectory, Foundation.NSSearchPathDomain.User)[0].Path!;
        Assert.IsTrue(Path.IsPathFullyQualified(directory));
    }
}
// endsnippet:complete
