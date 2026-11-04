' snippet:complete
Imports System.Drawing
Imports System.Windows.Forms

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Friend Class BriefingForm
    Inherits Form
    Private Sub InitializeComponent()
        confirm = New Button()
        SuspendLayout()
        confirm.Name = "confirm"
        confirm.Text = "Confirm reading"
        confirm.Location = New Point(20, 20)
        confirm.Size = New Size(180, 32)
        Controls.Add(confirm)
        ClientSize = New Size(360, 180)
        Text = "Fen instrument briefing"
        ResumeLayout(False)
    End Sub
    Friend WithEvents confirm As Button
End Class
' endsnippet:complete
