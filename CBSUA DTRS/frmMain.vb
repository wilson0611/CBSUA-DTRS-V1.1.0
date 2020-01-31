Public Class frmMain
    Private btns() As Button

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        lblTime.Text = Date.Now.ToShortTimeString
        lblDate.Text = Now.ToLongDateString.ToUpper

        If is_admin Then
            Reg_show()
            Exit Sub
        End If

        If strText IsNot vbNullString Then
            lblText.Text = "- " & strText.ToUpper & " -"
            lblLogTime.Text = strMsg
            If My.Computer.FileSystem.FileExists(Application.StartupPath & "\images\employees\" & client_ID & ".jpg") Then
                PictureBox2.ImageLocation = Application.StartupPath & "\images\employees\" & client_ID & ".jpg"
            Else
                PictureBox2.ImageLocation = Application.StartupPath & "\images\employees\avatar.jpg"
            End If
            Count += 1
        Else
            If Date.Now.Hour < 12 Then
                lblText.Text = "- " & "Good Morning!".ToUpper & " -"
            ElseIf Date.Now.Hour = 12 Then
                lblText.Text = "- " & "Good Noon".ToUpper & " -"
            Else
                lblText.Text = "- " & "Good Afternoon".ToUpper & " -"
            End If
            lblLogTime.Text = "Scanner Status: READY"
        End If

        If Count > 20 Then
            strText = vbNullString
            client_ID = vbNullString
            PictureBox2.ImageLocation = ""
            PictureBox2.Image = My.Resources.f3
            PictureBox2.Refresh()
            pbFingerprint.Visible = False
            Count = 0
        End If

        If type = "IN" Then
            Button1.PerformClick()
        Else
            Button2.PerformClick()
        End If

        ' CHECK AUTO SYNC OPTION
        If CDate(settings("sync_time")).ToString("h:mmtt") = Now.ToString("h:mmtt") Then
            ' EXECUTE SYNC
        End If
    End Sub

    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        If CheckConnection(1) Then
            UpdateDatabase()
            is_online = True
        Else
            is_online = False
        End If
    End Sub
    Private Sub Form3_Load(sender As Object, e As EventArgs) Handles Me.Load
        btns = {Button1, Button2, Button3, Button4}
        fpBox = pbFingerprint
        dgReport = DataGridView1
        frmForm = Me

        ShowRecentLog()
    End Sub

    Private Sub PictureBox3_Click(sender As Object, e As EventArgs) Handles PictureBox3.Click
        'verify_admin = True
        'pnlVerifyAdmin.Visible = True
        'fpBox = pbAdmin

        Reg_show()
    End Sub

    Private Sub Buttons_Click(sender As Object)
        For i = 0 To btns.Count - 1
            btns(i).BackColor = Color.Gainsboro
            btns(i).ForeColor = Color.DimGray
        Next

        If sender Is Button1 Or sender Is Button3 Then
            sender.BackColor = Color.ForestGreen
            type = "IN"
        Else
            sender.BackColor = Color.Red
            type = "OUT"
        End If
        sender.ForeColor = Color.White
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Buttons_Click(sender)
        btnActive = 2
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Buttons_Click(sender)
        btnActive = 1
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Buttons_Click(sender)
        btnActive = 4
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Buttons_Click(sender)
        btnActive = 3
    End Sub

    Public Sub Reg_show()
        frmIndex = 1
        frmForm = Form1
        Form1.Show()
        Hide()
        is_admin = False
        verify_admin = False
        pnlVerifyAdmin.Visible = False
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        verify_admin = False
        pnlVerifyAdmin.Visible = False
        fpBox = pbFingerprint
    End Sub

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        'GetReaders()
    End Sub

End Class