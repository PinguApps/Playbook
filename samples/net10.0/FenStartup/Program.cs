// snippet:complete
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        using Form window = new() { Text = "Fen daily observations" };
        window.Controls.Add(new Label { Text = "Observation queue prepared before first presentation", AutoSize = true });
        window.Shown += (_, _) => { Console.WriteLine("PASS older C# prepared content and Shown"); window.Close(); };
        Application.Run(window);
    }
}
// endsnippet:complete
