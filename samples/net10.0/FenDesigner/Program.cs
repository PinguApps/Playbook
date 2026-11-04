// snippet:complete
using System.Drawing;
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        using BriefingForm window = new();
        window.Shown += (_, _) => { window.CheckClick(); window.Close(); };
        Application.Run(window);
    }
}

internal partial class BriefingForm : Form
{
    private int confirmations;
    private readonly List<string> readings = new();
    internal BriefingForm() { InitializeComponent(); }
    internal IReadOnlyList<string> Readings => readings;
    internal List<string> CreateReadingsSnapshot() => new(readings);
    private void ConfirmClick(object? sender, EventArgs e) { confirmations++; }
    internal void CheckClick()
    {
        confirm.PerformClick();
        if (confirmations != 1 || !ReferenceEquals(Readings, Readings) || ReferenceEquals(CreateReadingsSnapshot(), CreateReadingsSnapshot()))
            throw new InvalidOperationException("Event wiring or intended identity failed.");
        Console.WriteLine("PASS named event handler, stable getter and independently created snapshots. Designer reopen not tested.");
    }
}
// endsnippet:complete
