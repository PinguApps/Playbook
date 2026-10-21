using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using Playbook.Samples;

foreach ((string method, string syntax) in new[] { ("CSharp", "C#"), ("FSharp", "F#"), ("VisualBasic", "Visual Basic") })
{
    MethodInfo member = typeof(LessonDraft).GetMethod(method)!;
    StringSyntaxAttribute annotation = member.GetParameters()[0].GetCustomAttribute<StringSyntaxAttribute>()!;
    if (annotation.Syntax != syntax) { throw new InvalidOperationException($"Incorrect {method} syntax metadata: expected {syntax}, actual {annotation.Syntax}."); }
    if ((string)member.Invoke(null, ["deliberately invalid source !!!"])! != "deliberately invalid source !!!")
    {
        throw new InvalidOperationException("An annotation must not validate or execute source.");
    }
}

foreach ((string format, string expected) in new[] { ("mp4", "video/mp4"), ("mpeg", "video/mpeg"), ("ogg", "video/ogg"), ("mov", "video/quicktime"), ("webm", "video/webm") })
{
    ClipDescription embedded = HarbourClips.Describe("DATA:video/mp4;base64,AA==", format);
    ClipDescription remote = HarbourClips.Describe("https://harbour.example/clips/training", format);
    if (!embedded.Embedded || remote.Embedded || embedded.DeclaredMediaType != expected || remote.DeclaredMediaType != expected)
    {
        throw new InvalidOperationException("Clip description mismatch.");
    }
}

foreach ((string address, string format) in new[] { ("relative/clip", "mp4"), ("https://harbour.example/clip", "unknown") })
{
    try { HarbourClips.Describe(address, format); }
    catch (ArgumentException) { continue; }
    throw new InvalidOperationException("Invalid input was accepted.");
}

// Scheme recognition makes no promise that an embedded payload is valid or matches the caller's label.
if (!HarbourClips.Describe("data:not-a-media-payload", "webm").Embedded)
{
    throw new InvalidOperationException("Scheme recognition unexpectedly validated the payload.");
}

Console.WriteLine($"PASS: syntax metadata (C#/F#/VB), no runtime validation, data scheme and five media types; {RuntimeInformation.FrameworkDescription}.");
