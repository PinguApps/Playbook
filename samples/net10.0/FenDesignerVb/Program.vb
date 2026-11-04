' snippet:complete
Imports System
Imports System.Collections.Generic
Imports System.Windows.Forms

Friend Module EntryPoint
    <STAThread>
    Public Sub Main()
        Application.EnableVisualStyles()
        Using window As New BriefingForm()
            AddHandler window.Shown, Sub(sender, args)
                                         window.CheckClick()
                                         window.Close()
                                     End Sub
            Application.Run(window)
        End Using
    End Sub
End Module

Partial Friend Class BriefingForm
    Private confirmations As Integer
    Private ReadOnly values As New List(Of String)()
    Public Sub New()
        InitializeComponent()
    End Sub
    Friend ReadOnly Property Readings As IReadOnlyList(Of String)
        Get
            Return values
        End Get
    End Property
    Friend Function CreateReadingsSnapshot() As List(Of String)
        Return New List(Of String)(values)
    End Function
    Private Sub ConfirmClick(sender As Object, args As EventArgs) Handles confirm.Click
        confirmations += 1
    End Sub
    Friend Sub CheckClick()
        confirm.PerformClick()
        If confirmations <> 1 OrElse Not Object.ReferenceEquals(Readings, Readings) OrElse Object.ReferenceEquals(CreateReadingsSnapshot(), CreateReadingsSnapshot()) Then
            Throw New InvalidOperationException("Event wiring or intended identity failed.")
        End If
        Console.WriteLine("PASS VB Handles event, stable getter and distinct snapshots. Designer reopen not tested.")
    End Sub
End Class
' endsnippet:complete
