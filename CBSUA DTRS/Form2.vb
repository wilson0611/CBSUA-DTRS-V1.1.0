Public Class Form2
    Dim settings As New List(Of List(Of String))
    Dim config(,) As String = {{"server", "localhost"}, {"username", "root"}, {"password", ""}, {"interval", "5"}, {"online", "0"}}

    Sub DefaultConfig()
        settings.Add(New List(Of String))
        settings.Add(New List(Of String))

        dbHost = "localhost"
        dbUser = "root"
        dbPass = ""

        Dim sw As New IO.StreamWriter(IO.File.Open("config.cfg", IO.FileMode.Open))
        sw.WriteLine("host" & vbTab & dbHost)
        sw.WriteLine("user" & vbTab & dbUser)
        sw.WriteLine("pass" & vbTab & dbPass)
        sw.Close()
    End Sub

    Sub LoadConfig()
        If Not My.Computer.FileSystem.FileExists("config.cfg") Then
            Dim file = IO.File.Create("config.cfg")
            file.Close()
            DefaultConfig()
        End If

        Dim sw As New IO.StreamReader(IO.File.Open("config.cfg", IO.FileMode.Open))

        Dim i As Byte = 0
        While i < config.Length - 1
            MsgBox(config(i, 1))
            i += 1
        End While
    End Sub

    Sub SaveConfig()

    End Sub

    Private Sub Form2_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadConfig()
    End Sub
End Class