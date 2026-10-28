// snippet:complete
namespace WillowQuay;

// Local test observations only; not application authorization or business state.
public sealed class PanelAudit
{
    private int timerFired;
    private int disposedPanels;
    public int TimerFired => Volatile.Read(ref timerFired);
    public int DisposedPanels => Volatile.Read(ref disposedPanels);
    public void TimerCompleted() => Interlocked.Increment(ref timerFired);
    public void PanelDisposed() => Interlocked.Increment(ref disposedPanels);
}
// endsnippet:complete
