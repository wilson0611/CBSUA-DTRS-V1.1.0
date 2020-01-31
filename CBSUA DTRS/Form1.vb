Public Class Form1
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles btnAttendance.Click, btnRegistration.Click, btnSetting.Click
        Dim ctr As New UserControl

        If sender Is btnAttendance Then
            ctr = New ctrlEmployee
        ElseIf sender Is btnSetting Then
            ctr = New ctrlSettings
        Else
            ctr = New ctrlRegistration
        End If
        ctr.Dock = DockStyle.Fill

        Panel6.Controls.Clear()
        Panel6.Controls.Add(ctr)
        ActiveButton(sender)
    End Sub

    Private Sub ActiveButton(sender As Object)
        btnAttendance.BackColor = Color.FromArgb(47, 47, 49)
        btnRegistration.BackColor = Color.FromArgb(47, 47, 49)
        btnSetting.BackColor = Color.FromArgb(47, 47, 49)

        sender.BackColor = Color.ForestGreen
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        frmMain.Show()
        Dispose()
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
        frmForm = frmMain
        frmIndex = 0

        Dim ctr As New ctrlRegistration
        Panel6.Controls.Add(ctr)
    End Sub
    Private Sub Form1_disposed(sender As Object, e As EventArgs) Handles Me.Disposed
        fpBox = frmMain.pbFingerprint
        frmIndex = 0
    End Sub
End Class