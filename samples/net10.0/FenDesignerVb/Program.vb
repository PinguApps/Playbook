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
' endsnippet:complete
