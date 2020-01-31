Imports System.ComponentModel
Imports System.IO
Imports System.Speech.Synthesis
Imports System.Text

Public Class Form3
    Public Narrator As New SpeechSynthesizer()

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        BackgroundWorker1.RunWorkerAsync()
    End Sub

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        Console.WriteLine(Now)
        Dim att As New Attendance

        att.Start("3", "01", "2020")
    End Sub

    Private Sub BackgroundWorker1_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted
        Console.WriteLine(Now)
    End Sub

    Private Sub Form3_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        GetSettings()
        If CheckConnection(1) Then
            is_online = True
            Console.WriteLine("Online database initiated")
        End If
    End Sub

    Sub GetSettings()
        settings.Clear()

        db_master = Database("bio_db_master")
        command.Connection = db_master
        command.CommandText = "SELECT * FROM tbl_settings"
        dbreader = command.ExecuteReader()
        If dbreader.HasRows Then
            While dbreader.Read
                settings.Add(dbreader.Item(1), dbreader.Item(2))
            End While
        End If
        dbreader.Close()
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        If Date.Compare(Now.ToString("H:m:s"), settings("sync_time")) = 0 Then
            Label1.Text = "Processing..."
            BackgroundWorker1.RunWorkerAsync()
        Else
            Label1.Text = "Idle..."
        End If
    End Sub

    Sub Upload(data As String)
        Dim url As String = "https://dtrs.cbsuainfotech.com"
        Try
            Dim request As Net.WebRequest = Net.WebRequest.Create(url)
            request.Method = "POST"
            Dim postData = data
            Dim byteArray As Byte() = Encoding.UTF8.GetBytes(postData)
            request.ContentType = "application/x-www-form-urlencoded"
            request.ContentLength = byteArray.Length
            Dim dataStream As Stream = request.GetRequestStream()
            dataStream.Write(byteArray, 0, byteArray.Length)
            dataStream.Close()
        Catch ex As Exception
            Console.WriteLine(ErrorToString)
        End Try
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If CheckConnection(1) Then
            MsgBox("Connected")
        Else
            MsgBox("Not Connected")
        End If
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Narrator.SpeakAsyncCancelAll()
        Narrator.SelectVoiceByHints(VoiceGender.Male)
        Narrator.Rate = 1
        Narrator.SpeakAsync("Failed!")
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Upload("emp[EmployeeID]=19-001&emp[FirstName]=Juan&emp[LastName]=dela%20Cruz")
    End Sub
End Class