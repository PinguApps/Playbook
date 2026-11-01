using Microsoft.Extensions.Options;
namespace Playbook.Samples;
public static class Schedule { public static string Label(IOptions<string> slot) => "show:" + slot.Value; }
