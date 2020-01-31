Imports System.ComponentModel
Imports MySql.Data.MySqlClient

Public Class EmployeeList
    Dim Employees, EType As New DataTable

    Private Sub EmployeeList_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        BackgroundWorker1.RunWorkerAsync()
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
        ShowEmployees()
    End Sub

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        ' LOAD ALL EMPLOYEES
        'Employees = DTResult("SELECT EmployeeID, LastName, FirstName, MiddleName FROM tbl_employee WHERE ActiveStatus = 1 AND CampusID = '" & campus_id & "' ORDER BY LastName ASC", db_master)

        Employees = DTResult("SELECT tbl_employee.EmployeeID, tbl_employee.LastName, tbl_employee.FirstName, tbl_employee.MiddleName FROM tbl_employee WHERE tbl_employee.ActiveStatus = 1 AND tbl_employee.CampusID = '" & campus_id & "' AND NOT EXISTS(SELECT * FROM tbl_fingerprint WHERE tbl_employee.EmployeeID = tbl_fingerprint.emp_id) ORDER BY LastName ASC", db_master)
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If DataGridView1.SelectedRows.Count > 0 Then
            newEmpId = DataGridView1.Item(0, DataGridView1.SelectedRows.Item(0).Index).Value.ToString
            SetID()
            Dispose()
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dispose()
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        ShowEmployees()
    End Sub

    Private Sub BackgroundWorker1_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted
        DataGridView1.DataSource = Employees
    End Sub

    Sub ShowEmployees()
        If CheckBox1.Checked Then
            DataGridView1.DataSource = DTResult("SELECT tbl_employee.EmployeeID, tbl_employee.LastName, tbl_employee.FirstName, tbl_employee.MiddleName FROM tbl_employee WHERE tbl_employee.ActiveStatus = 1 AND tbl_employee.CampusID = '" & campus_id & "' AND LastName LIKE '%" & TextBox1.Text & "%' ORDER BY LastName ASC", db_master)
        Else
            DataGridView1.DataSource = DTResult("SELECT tbl_employee.EmployeeID, tbl_employee.LastName, tbl_employee.FirstName, tbl_employee.MiddleName FROM tbl_employee WHERE tbl_employee.ActiveStatus = 1 AND tbl_employee.CampusID = '" & campus_id & "' AND LastName LIKE '%" & TextBox1.Text & "%' AND NOT EXISTS(SELECT * FROM tbl_fingerprint WHERE tbl_employee.EmployeeID = tbl_fingerprint.emp_id) ORDER BY LastName ASC", db_master)
        End If
    End Sub
End Class