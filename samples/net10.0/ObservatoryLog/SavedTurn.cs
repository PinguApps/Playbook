// snippet:complete
namespace ObservatoryLog;
public sealed record SavedTurn(string Question, string Answer);
public sealed record SavedLog(string ThreadId, List<SavedTurn> Turns);
// endsnippet:complete
