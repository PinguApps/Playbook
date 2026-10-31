// snippet:alias
internal static class SurveyAlias
{
    internal static void Create(string aliasPath, string existingPath)
        => File.CreateHardLink(aliasPath, existingPath);

    // The receiver names the NEW alias; the argument names the EXISTING file.
    internal static void CreateWithInfo(string aliasPath, string existingPath)
        => new FileInfo(aliasPath).CreateAsHardLink(existingPath);
}
// endsnippet:alias
