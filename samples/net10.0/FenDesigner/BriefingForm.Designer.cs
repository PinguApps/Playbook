// snippet:complete
using System.Drawing;
using System.Windows.Forms;

internal partial class BriefingForm
{
    private void InitializeComponent()
    {
        confirm = new Button();
        SuspendLayout();
        confirm.Name = "confirm";
        confirm.Text = "Confirm reading";
        confirm.Location = new Point(20, 20);
        confirm.Size = new Size(180, 32);
        confirm.Click += ConfirmClick;
        Controls.Add(confirm);
        ClientSize = new Size(360, 180);
        Text = "Fen instrument briefing";
        ResumeLayout(false);
    }
    private Button confirm = null!;
}
// endsnippet:complete
