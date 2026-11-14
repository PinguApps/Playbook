// snippet:complete
namespace CwmSeedBank; public static class Germination { public static bool Ready(int viable, int total) => total > 0 && viable * 100L >= total * 80L; }
// endsnippet:complete
