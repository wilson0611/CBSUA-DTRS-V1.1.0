Public Class ctrlSettings
    Dim key(), value() As String

    Private Sub ctrlSettings_Load(sender As Object, e As EventArgs) Handles Me.Load
        LoadSettings()
    End Sub

    Private Sub LoadSettings()
        txtOffServer.Text = settings("offline_server_ip")
        txtOffUser.Text = settings("offline_server_user")
        txtOffPass.Text = settings("offline_server_password")

        txtOnServer.Text = settings("online_server_ip")
        txtOnUser.Text = settings("online_server_user")
        txtOnPass.Text = settings("online_server_password")

        numInterval.Value = settings("log_interval")

        If settings("auto_sync") = 1 Then
            CheckBox1.Checked = True
        Else
            CheckBox1.Checked = False
        End If
    End Sub

    Private Sub RefreshSettings()
        GetSettings()
        LoadSettings()
    End Sub

    Private Sub UpdateSetting(key() As String, value() As String)
        Dim stmt As String = ""
        For i = 0 To key.Count - 1
            stmt = stmt & "UPDATE tbl_settings SET value = '" & value(i) & "' WHERE keyword = '" & key(i) & "';"
        Next
        If ExecQuery(stmt, db_master) Then
            MsgBox("Successfully save.")
        End If
        RefreshSettings()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        key = {"offline_server_ip", "offline_server_user", "offline_server_password"}
        value = {txtOffServer.Text, txtOffUser.Text, txtOffPass.Text}
        UpdateSetting(key, value)
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        key = {"online_server_ip", "online_server_user", "online_server_password"}
        value = {txtOnServer.Text, txtOnUser.Text, txtOnPass.Text}
        UpdateSetting(key, value)
        ConnectOnlineDatabase()
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        key = {"log_interval"}
        value = {numInterval.Value}
        UpdateSetting(key, value)
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        key = {"auto_sync"}
        If CheckBox1.Checked Then
            value = {1}
        Else
            value = {0}
        End If
        UpdateSetting(key, value)
    End Sub
End Class
