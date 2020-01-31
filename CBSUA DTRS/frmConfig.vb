Public Class frmConfig
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        dbHost = txtHost.Text
        dbUser = txtUser.Text
        dbPass = txtPass.Text

        InitializeConnection()
        If Not conn.State = ConnectionState.Open Then
            Label4.Text = "STATUS: Unable to connect. Try again."
        Else
            Close()
        End If
    End Sub
End Class