Imports DPUruNet

Public Class ctrlRegistration
    Private id As String = "00000"
    Private emp_name As String
    Private _count As Byte

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        tbNotif.Text = strNotif
        lblMessage.Text = strError
        lblEmpName.Text = emp_name
    End Sub

    Private Sub RegistrationForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        fpBox = pbFingerprint
        txtID = txtEmpID
        frmIndex = 1
    End Sub

    Private Sub btnRegister_Click(sender As Object, e As EventArgs) Handles btnRegister.Click
        If _fprint(0) IsNot vbNullString OrElse _fprint(1) IsNot vbNullString Then
            If ValidID(txtEmpID.Text) Then
                RegisterClient(txtEmpID.Text)
            Else
                strError = "ERROR: Invalid employee number."
            End If
        Else
            strError = "ERROR: Empty fingerprints."
        End If
    End Sub

    Private Sub btnFprint1_Click(sender As Object, e As EventArgs) Handles btnFprint1.Click
        strNotif = "Place your finger"
        strFPrint1 = ""
        CheckCount = 0
        CheckBox1.Checked = True
    End Sub
    Private Sub btnFprint2_Click(sender As Object, e As EventArgs) Handles btnFprint2.Click
        strNotif = "Place your finger"
        strFPrint2 = ""
        CheckCount = 0
        CheckBox2.Checked = True
    End Sub

    Private Sub frmRegistration_Disposed(sender As Object, e As EventArgs) Handles Me.Disposed


    End Sub

    Sub RegisterClient(id As String)
        For Each x As String In _fprint
            If x IsNot vbNullString Then
                ExecQuery("INSERT INTO `tbl_fingerprint`(`emp_id`, `xml`) VALUES ('" & id & "','" & x & "')", db_master)
                If is_online Then
                    ExecQuery("INSERT INTO `tbl_fingerprint`(`emp_id`, `xml`) VALUES ('" & id & "','" & x & "')", dtrs_master)
                End If
            End If
        Next
        strNotif = "SUCCESS"
        strError = "Successfully registered."
        Reset()
    End Sub

    Private Sub Reset()
        _fprint(0) = vbNullString
        _fprint(1) = vbNullString
        strError = ""
        emp_name = ""
        GroupBox3.Enabled = False
        LoadFingerprints()
        tbID.Text = "00000"
        id = "00000"

        CheckBox1.Checked = False
        CheckBox2.Checked = False

        fpBox.Image = My.Resources.f0
        fpBox.Visible = False

        _flag = False
    End Sub

    Private Sub CheckBox1_CheckedChanged() Handles CheckBox1.CheckedChanged
        If CheckBox1.Checked = True Then
            index = 0
            btnFprint1.Enabled = True
        Else
            _fprint(0) = vbNullString
            btnFprint1.Enabled = False
        End If
        _flag = CheckFlag()
    End Sub

    Private Sub CheckBox2_CheckedChanged() Handles CheckBox2.CheckedChanged
        If CheckBox2.Checked = True Then
            index = 1
            btnFprint2.Enabled = True
        Else
            _fprint(1) = vbNullString
            btnFprint2.Enabled = False
        End If

        _flag = CheckFlag()
    End Sub

    Function CheckFlag() As Boolean
        Return CheckBox1.Checked Or CheckBox2.Checked
    End Function

    Public Sub setEmp(sender As Object, e As EventArgs) Handles txtEmpID.TextChanged

        If Not ValidID(txtEmpID.Text) Then
            Reset()
            strError = "ERROR: Invalid employee number."
        Else
            _flag = True
            GroupBox3.Enabled = True
            fpBox.Image = My.Resources.f0
            fpBox.Visible = True
            strError = vbNullString

            CheckBox1.Checked = True
        End If
    End Sub

    Function ValidID(id As String) As Boolean
        If Not dbreader.IsClosed Then dbreader.Close()

        command.Connection = db_master
        command.CommandText = "SELECT FirstName, LastName, MiddleName FROM tbl_employee WHERE EmployeeID = '" & id & "' LIMIT 0,1"
        dbreader = command.ExecuteReader()

        If dbreader.HasRows Then
            While dbreader.Read
                lblFName.Text = dbreader.Item(0)
                lblMName.Text = dbreader.Item(1)
                lblLName.Text = dbreader.Item(2)
                'emp_name = dbreader.Item(0) & " " & dbreader.Item(1)
            End While
            dbreader.Close()
            Return True
        End If
        Return False
    End Function

    Private Sub Button13_Click(sender As Object, e As EventArgs) Handles Button13.Click
        EmployeeList.ShowDialog()
    End Sub

    Private Sub Label1_Click(sender As Object, e As EventArgs) Handles Label1.Click

    End Sub
End Class
