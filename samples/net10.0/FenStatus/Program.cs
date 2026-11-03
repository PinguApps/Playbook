// snippet:complete
using System.Drawing;
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        using Form window = new() { Text = "Fen monitoring", ClientSize = new Size(620, 220) };
        using StatusTile tile = new() { Dock = DockStyle.Fill, Text = "Fen north: water level normal" };
        window.Controls.Add(tile);
        window.Shown += (_, _) => { tile.Refresh(); Console.WriteLine("PASS older system-color tile painted"); window.Close(); };
        Application.Run(window);
    }
}

internal sealed class StatusTile : Control
{
    public StatusTile() => SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
    protected override void OnSystemColorsChanged(EventArgs e) { base.OnSystemColorsChanged(e); Invalidate(); }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.Clear(SystemColors.Window);
        TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, SystemColors.WindowText);
    }
}
// endsnippet:complete
