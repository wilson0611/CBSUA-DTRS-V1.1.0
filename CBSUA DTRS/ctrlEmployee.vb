Imports System.ComponentModel

Public Class ctrlEmployee
    Dim employees As New DataTable

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        employees = DTResult("SELECT EmployeeID, LastName, FirstName, MiddleName, ExtName FROM tbl_employee WHERE ActiveStatus = 1 AND CampusId = " & campus_id, db_master)
    End Sub

    Private Sub BackgroundWorker1_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted
        DataGridView1.DataSource = employees
    End Sub

    Private Sub ctrlEmployee_Load(sender As Object, e As EventArgs) Handles Me.Load
        BackgroundWorker1.RunWorkerAsync()
    End Sub

End Class
