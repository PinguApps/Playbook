// snippet:complete
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        using Form window = new() { Text = "Fen observation queue" };
        using FlowLayoutPanel group = new() { Dock = DockStyle.Fill };
        using ListBox rows = new() { Width = 260 };
        group.Controls.Add(rows);
        window.Controls.Add(group);
        window.Shown += (_, _) =>
        {
            group.SuspendLayout();
            rows.BeginUpdate();
            try
            {
                group.Padding = new Padding(12);
                rows.Items.Add("Fen north: sample collected");
            }
            finally { rows.EndUpdate(); group.ResumeLayout(true); }
            rows.Items.Add("Fen south: ready after update");
            rows.Refresh();
            if (rows.Items.Count != 2) throw new InvalidOperationException("Queue update failed.");
            Console.WriteLine("PASS older paired layout/update cleanup and subsequent mutation");
            window.Close();
        };
        Application.Run(window);
    }
}
// endsnippet:complete
