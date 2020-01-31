Imports System.ComponentModel
Imports System.Threading

Public Class frmLoading
    Dim info As String

    Private Sub loadingForm_load(sender As Object, e As EventArgs) Handles MyBase.Load
        BackgroundWorker1.RunWorkerAsync()
    End Sub

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        info = "Initializing fingerprint readers...."
        GetReaders()


        info = "Initializing database connection...."
        InitializeConnection() 'Iniatialize connection
        If Not conn.State = ConnectionState.Open Then
            frmConfig.ShowDialog()
        End If

        info = "Initializing database...."
        db_master = Database("bio_db_master")
        db_raw_data = Database("bio_db_raw_data")
        db_attendance = Database("bio_db_attendance")

        info = "Initializing system settings...."
        GetSettings()

        info = "Initializing online database...."
        ConnectOnlineDatabase()

        info = "Initializing employees fingerprint...."
        LoadFingerprints()
        If Not CheckTable(Format(Now, "MM-yyyy")) Then GenerateTable()
    End Sub

    Private Sub BackgroundWorker1_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted
        Hide()
        'Form1.Show()
        frmMain.Show()
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Label1.Text = info
    End Sub
End Class