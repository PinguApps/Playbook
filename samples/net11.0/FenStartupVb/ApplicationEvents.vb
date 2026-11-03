' snippet:complete
Imports Microsoft.VisualBasic.ApplicationServices
Imports System.Windows.Forms

Friend Module EntryPoint
    <STAThread>
    Public Sub Main()
        Dim app As New FenApplication()
        app.Run(Array.Empty(Of String)())
    End Sub
End Module

Friend Partial Class FenApplication
    Inherits WindowsFormsApplicationBase

    Public Sub New()
        MyBase.New(AuthenticationMode.Windows)
        EnableVisualStyles = True
    End Sub

    Private Sub ConfigureDefaults(sender As Object, e As ApplyApplicationDefaultsEventArgs) Handles Me.ApplyApplicationDefaults
        ' VB retains Classic defaults unless ApplicationEvents deliberately changes them.
        e.FormRevealMode = System.Windows.Forms.FormRevealMode.Deferred
        e.VisualStylesMode = System.Windows.Forms.VisualStylesMode.Net11
    End Sub

    Protected Overrides Sub OnCreateMainForm()
        MainForm = New Form With {.Text = "Fen VB observation desk"}
        MainForm.Controls.Add(New Label With {.Text = "Observations prepared by VB startup", .AutoSize = True})
        AddHandler MainForm.Shown, Sub(sender, e)
                                      If Not Application.IsFormRevealDeferred Then Throw New InvalidOperationException("VB default was not applied.")
                                      Console.WriteLine("PASS VB ApplicationEvents deferred defaults and Shown")
                                      MainForm.Close()
                                  End Sub
    End Sub
End Class
' endsnippet:complete
