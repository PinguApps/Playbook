// snippet:complete
using System.Drawing;
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        Application.EnableVisualStyles();
        Application.SetDefaultVisualStylesMode(VisualStylesMode.Net11);
        using Form window = new() { Text = "Fen monitoring — accessibility check", ClientSize = new Size(620, 220) };
        using StatusTile tile = new() { Dock = DockStyle.Fill, Text = "Fen north: water level normal", AccessibleName = "Fen north water level" };
        window.Controls.Add(tile);
        window.Shown += (_, _) =>
        {
            tile.VisualStylesMode = VisualStylesMode.Classic;
            if (tile.ResolvedMode != VisualStylesMode.Classic) throw new InvalidOperationException("Classic selection was not resolved.");
            tile.VisualStylesMode = VisualStylesMode.Inherit;
            if (tile.VisualStylesMode != VisualStylesMode.Inherit) throw new InvalidOperationException("Requested inheritance was lost.");
            tile.Refresh();
            tile.Report("initial");
            Console.WriteLine("PASS real control renderer changes and paint; settings transitions require PIN-897");
            if (!args.Contains("--manual")) window.Close();
        };
        Application.Run(window);
    }
}

internal sealed class StatusTile : Control
{
    public VisualStylesMode ResolvedMode => EffectiveVisualStylesMode;

    public StatusTile()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        TabStop = true;
    }

    protected override VisualStylesModeChangeImpact GetVisualStylesModeChangeImpact(VisualStylesMode oldMode, VisualStylesMode newMode)
        => VisualStylesModeChangeImpact.Repaint;

    protected override void OnVisualStylesModeChanged(EventArgs e)
    {
        base.OnVisualStylesModeChanged(e);
        Invalidate();
    }

    protected override void OnSystemVisualSettingsChanged(SystemVisualSettingsChangedEventArgs e)
    {
        base.OnSystemVisualSettingsChanged(e);
        Console.WriteLine($"SETTINGS changed={e.Changed}");
        Report("notification");
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        SystemVisualSettings settings = Application.SystemVisualSettings;
        // System colors retain the owner's contrast scheme. Accent is decoration only.
        e.Graphics.Clear(SystemColors.Window);
        TextRenderer.DrawText(e.Graphics, Text, Font, new Rectangle(24, 32, Width - 48, Height - 64), SystemColors.WindowText);
        if (!settings.HighContrastEnabled && EffectiveVisualStylesMode == VisualStylesMode.Net11)
        {
            using SolidBrush accent = new(settings.AccentColor);
            e.Graphics.FillRectangle(accent, 8, 8, 6, Height - 16);
        }
        if (Focused) ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(ClientRectangle, -4, -4));
    }

    protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
    protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }

    public void Report(string reason)
        => Console.WriteLine($"STATE {reason}: highContrast={Application.SystemVisualSettings.HighContrastEnabled}; requested={VisualStylesMode}; effective={EffectiveVisualStylesMode}; runtime={System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
}
// endsnippet:complete
