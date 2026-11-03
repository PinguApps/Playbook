// snippet:complete
using System.Drawing;
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        Application.EnableVisualStyles();
        using Form window = new() { Text = "Fen observation queue" };
        using ObservationPanel group = new() { Dock = DockStyle.Fill };
        using ListBox rows = new() { Width = 260 };
        using Font readable = new(SystemFonts.MessageBoxFont!.FontFamily, 12);
        group.Controls.Add(rows);
        window.Controls.Add(group);
        window.Shown += (_, _) =>
        {
            int childLayouts = 0;
            rows.Layout += (_, _) => childLayouts++;
            try
            {
                using IDisposable batch = group.SuspendPainting(LayoutSuspendTraversal.TargetAndDescendants);
                group.Font = readable;
                group.Padding = new Padding(12);
                rows.PerformLayout();
                if (childLayouts != 0) throw new InvalidOperationException("Descendant layout was not suspended.");
                rows.Items.Add("Fen north: sample collected");
                throw new InvalidOperationException("simulated import failure");
            }
            catch (InvalidOperationException e) when (e.Message == "simulated import failure") { }
            if (group.Depth != 0 || group.Begins == 0 || group.Begins != group.Ends) throw new InvalidOperationException("Painting suspension leaked.");
            int layouts = 0;
            group.Layout += (_, _) => layouts++;
            group.PerformLayout();
            rows.Items.Add("Fen south: ready after failure");
            rows.Refresh();
            if (layouts == 0 || rows.Items.Count != 2) throw new InvalidOperationException("Updates did not resume.");
            Console.WriteLine($"PASS descendant suspension and exception cleanup: begin={group.Begins}, end={group.Ends}, layout={layouts}, rows={rows.Items.Count}");
            window.Close();
        };
        Application.Run(window);
    }
}

// Instrumentation belongs in the custom control, not in the mutation caller.
internal sealed class ObservationPanel : FlowLayoutPanel
{
    public int Depth { get; private set; }
    public int Begins { get; private set; }
    public int Ends { get; private set; }
    protected override void BeginSuspendPaintingCore() { base.BeginSuspendPaintingCore(); Depth++; Begins++; }
    protected override void EndSuspendPaintingCore() { base.EndSuspendPaintingCore(); Depth--; Ends++; }
}
// endsnippet:complete
