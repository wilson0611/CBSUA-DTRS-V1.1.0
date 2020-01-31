Imports DPUruNet
Imports DPUruNet.Constants
Imports MySql.Data.MySqlClient

Module Functions


    Sub LoadFingerprints()
        If Not dbreader.IsClosed Then dbreader.Close()
        command.Connection = db_master
        command.CommandText = "SELECT emp_id, xml FROM `tbl_fingerprint` WHERE status = '1'"
        dbreader = command.ExecuteReader()
        If dbreader.HasRows Then
            _fingerprints.Clear()
            Dim fprint As FingerPrints
            While dbreader.Read
                fprint.emp_id = dbreader.Item(0)
                fprint.fprint = dbreader.Item(1)
                _fingerprints.Add(fprint)
            End While
        End If
        dbreader.Close()
    End Sub

    Function Schedule(id As String)
        Dim scheds(4) As String
        Return scheds
    End Function

    Sub InitConnection()

    End Sub

    Function CheckConnection(type As Byte) As Boolean
        Dim conn As MySqlConnection
        If type = 0 Then
            'conn = New MySqlConnection("server=" & settings("offline_server_ip") & "; user=" & settings("offline_server_user") & "; password=" & settings("offline_server_password") & "; database=dtrs_attendance;")
            conn = New MySqlConnection("server=" & settings("offline_server_ip") & "; user=" & settings("offline_server_user") & "; password=" & settings("offline_server_password"))
        Else
            conn = New MySqlConnection("server=" & settings("online_server_ip") & "; port=3306; user id=" & settings("online_server_user") & ";password=" & settings("online_server_password"))
        End If

        Try
            conn.Open()
        Catch ex As Exception
            Return False
        End Try
        Return True
    End Function

    Sub InitializeConnection()
        conn = New MySqlConnection("server=" & dbHost & "; user=" & dbUser & "; password=" & dbPass)
        Try
            conn.Open()
        Catch ex As Exception
            MessageBox.Show(ex.Message + " Please check if Database has been started", "Cannot access database")
        End Try
    End Sub

    Function Database(dbname As String)
        Dim con As New MySqlConnection("server=" & dbHost & "; user=" & dbUser & "; password=" & dbPass & "; database=" & dbname)
        Try
1:
            con.Open()
            Return con
        Catch ex As Exception
            CreateDatabase(dbname)
            GoTo 1
        End Try
        Return vbNull
    End Function

    Function Online_Database(dbname As String)
        Dim con As New MySqlConnection("server=" & settings("online_server_ip") & "; port=3306; user id=" & settings("online_server_user") & ";password=" & settings("online_server_password") & "; database=" & dbname)
        Try
1:
            con.Open()
            Return con
        Catch ex As Exception
            CreateDatabase(dbname)
            GoTo 1
        End Try
        Return vbNull
    End Function

    Sub CreateDatabase(dbname As String)
        command = New MySqlCommand("CREATE DATABASE " & dbname, conn)
        command.ExecuteNonQuery()
    End Sub

    Function Exec(query As String)
        Dim result() As String = {""}
        Return result
    End Function

    Function Fix(id As String) As String
        While id.Length <= 4
            id = "0" & id
        End While
        Return id
    End Function

    Function ValidInterval(id As String) As Boolean
        If Not dbreader.IsClosed Then dbreader.Close()
        command.Connection = db_raw_data
        command.CommandText = "SELECT TIME_TO_SEC(TIMEDIFF(DATE_FORMAT(NOW(), '%H:%i:%s'), DATE_FORMAT(log_time, '%H:%i:%s'))) / 60 FROM `" & bioTableName & "`, (SELECT MAX(log_no) AS tID FROM `" & bioTableName & "` WHERE emp_id = '" & id & "' AND log_date = DATE_FORMAT(NOW(), '%Y-%m-%d')) AS tmp WHERE log_no = tID"
        dbreader = command.ExecuteReader()
        If dbreader.HasRows Then
            dbreader.Read()
            If CInt(dbreader.Item(0)) >= settings("log_interval") Then
                dbreader.Close()
                Return True
            Else
                dbreader.Close()
                Return False
            End If
        End If
        dbreader.Close()
        Return True
    End Function

    Function ValidateFPrint(fmdFPrint As Fmd) As Boolean
        For i = 0 To _fingerprints.Count - 1
            If Validate(_fingerprints(i).fprint, fmdFPrint) Then
                client_ID = _fingerprints(i).emp_id
                Return True
            End If
        Next
        Return False
    End Function

    Function DTResult(stmt As String, conn As MySqlConnection) As DataTable

        If Not dbreader.IsClosed Then dbreader.Close()
        Try
            Dim result As New DataTable
            command = New MySqlCommand(stmt, conn)
            adapter = New MySqlDataAdapter(command)
            adapter.Fill(result)
            Return result
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try
    End Function

    Function RegValidateFPrint(fmdFPrint As Fmd) As Boolean
        For i = 0 To _fingerprints.Count - 1
            If Validate(_fingerprints(i).fprint, fmdFPrint) Then
                Return True
            End If
        Next
        Return False
    End Function

    Sub Verify(fmdFPrint As Fmd)
        If frmIndex = 0 Then
            If ValidateFPrint(fmdFPrint) Then
                If verify_admin AndAlso IsAdmin(client_ID) Then
                    is_admin = True
                Else
                    If ValidInterval(client_ID) Then
                        ShowData(client_ID)
                        SaveLog()
                        strText = clientName
                        strMsg = "LOG " & type & ": " & Now.ToShortTimeString
                    Else
                        strText = "Invalid Interval"
                        strMsg = "INVALID INTERVAL: Please allow at-least " & settings("log_interval") & " minutes log interval."
                    End If
                End If
            Else
                strText = "Failed"
                strMsg = "ERROR: Fingerprint doesn't match. Try again."
            End If
            Count = 0
        Else
            If _flag Then
                strNotif = ""
                strError = ""

                If CheckCount = 0 Then
                    fingerprint1 = fmdFPrint
                    If Not RegValidatePrint(fmdFPrint) AndAlso Not RegValidateFPrint(fmdFPrint) Then
                        strNotif = "Place the same finger"
                        CheckCount += 1
                    Else
                        strNotif = "FAILED"
                        strError = "ERROR: Fingerprint already registered."
                    End If
                ElseIf CheckCount < 3 Then
                    If RegValidate(fmdFPrint) Then
                        strNotif = "Place the same finger"
                        CheckCount += 1
                    Else
                        strNotif = "FAILED"
                        strError = "ERROR: Pleace place the same finger."
                    End If
                Else
                    SaveFingerprint(Fmd.SerializeXml(fingerprint1))
                    strNotif = "Fingerprint " & index + 1 & " saved!"
                End If
            Else
                strNotif = "FAILED"
                strError = "ERROR: Please tick the checkbox first."
            End If
        End If
    End Sub

    Function RegValidatePrint(fprint As Fmd) As Boolean
        For i = 0 To _fprint.Count - 1
            If _fprint(i) <> "" AndAlso Validate(_fprint(i), fprint) Then
                Return True
            End If
        Next
        Return False
    End Function

    Function ValidLog() As Boolean
        If Not dbreader.IsClosed Then dbreader.Close()
        command.Connection = db_raw_data
        command.CommandText = "SELECT * FROM `" & bioTableName & "`, (SELECT MAX(log_no) AS mx FROM `" & bioTableName & "` WHERE emp_id = '" & client_ID & "') AS tmp WHERE log_no = mx AND log_type = " & IntType()
        dbreader = command.ExecuteReader()
        If dbreader.HasRows Then
            dbreader.Close()
            Return False
        End If
        dbreader.Close()
        Return True
    End Function

    Function IsFirst() As Boolean
        If Not dbreader.IsClosed Then dbreader.Close()
        command.Connection = db_raw_data
        command.CommandText = "SELECT * FROM `" & bioTableName & "` where emp_id = '" & client_ID & "' and log_date = '" & Now.ToString("yyyy-MM-d") & "'"
        dbreader = command.ExecuteReader()
        If dbreader.HasRows Then
            dbreader.Close()
            Return False
        End If
        Return True
    End Function

    Sub SaveLog()
        If ValidLog() Then
            ExecQuery("INSERT INTO `" & bioTableName & "` SET emp_id = '" & client_ID & "', log_date = DATE_FORMAT(NOW(), '%Y-%m-%d'), log_time = DATE_FORMAT(NOW(), '%H:%i:%s'), signature = SUBSTRING(MD5(RAND()) FROM 1 FOR 32), log_type = " & IntType() & ", campus_id = " & campus_id, db_raw_data)
            If is_online Then
                ExecQuery("INSERT INTO `" & bioTableName & "` SET emp_id = '" & client_ID & "', log_date = DATE_FORMAT(NOW(), '%Y-%m-%d'), log_time = DATE_FORMAT(NOW(), '%H:%i:%s'), signature = SUBSTRING(MD5(RAND()) FROM 1 FOR 32), log_type = " & IntType() & ", campus_id = " & campus_id, dtrs_raw_data)

            Else
                ExecQuery("INSERT INTO `unsaved` SET emp_id = '" & client_ID & "', log_date = DATE_FORMAT(NOW(), '%Y-%m-%d'), log_time = DATE_FORMAT(NOW(), '%H:%i:%s'), signature = SUBSTRING(MD5(RAND()) FROM 1 FOR 32), log_type = " & IntType() & ", campus_id = " & campus_id, db_raw_data)
            End If
        Else
            Dim msg As String
            Dim x As Byte = IntType()

            If x = 1 Then
                msg = "Did you mean OUT?"
                type = "OUT"
                x = 2
            Else
                msg = "Did you mean IN?"
                type = "IN"
                x = 1
            End If
            Display(msg, x)
        End If
        ShowRecentLog()
    End Sub

    Sub Display(msg As String, x As Byte)
        If frmForm.InvokeRequired Then
            frmForm.Invoke(Sub() Display(msg, x))
            Return
        End If
        Dim a = MessageBox.Show(msg, "Message", MessageBoxButtons.YesNoCancel)
        If a = MsgBoxResult.Yes Then
            ExecQuery("INSERT INTO `" & bioTableName & "` SET emp_id = '" & client_ID & "', log_date = DATE_FORMAT(NOW(), '%Y-%m-%d'), log_time = DATE_FORMAT(NOW(), '%H:%i:%s'), signature = SUBSTRING(MD5(RAND()) FROM 1 FOR 32), log_type = " & x & ", campus_id = " & campus_id, db_raw_data)
            If is_online Then
                If att.CheckAttendanceCount(client_ID, Now.ToString("YYYY-mm-dd")) Then

                End If
                ExecQuery("INSERT INTO `" & bioTableName & "` SET emp_id = '" & client_ID & "', log_date = DATE_FORMAT(NOW(), '%Y-%m-%d'), log_time = DATE_FORMAT(NOW(), '%H:%i:%s'), signature = SUBSTRING(MD5(RAND()) FROM 1 FOR 32), log_type = " & x & ", campus_id = " & campus_id, dtrs_raw_data)
            Else
                ExecQuery("INSERT INTO `unsaved` SET emp_id = '" & client_ID & "', log_date = DATE_FORMAT(NOW(), '%Y-%m-%d'), log_time = DATE_FORMAT(NOW(), '%H:%i:%s'), signature = SUBSTRING(MD5(RAND()) FROM 1 FOR 32), log_type = " & x & ", campus_id = " & campus_id, db_raw_data)
            End If
        ElseIf a = MsgBoxResult.No Then
            ExecQuery("INSERT INTO `" & bioTableName & "` SET emp_id = '" & client_ID & "', log_date = DATE_FORMAT(NOW(), '%Y-%m-%d'), log_time = DATE_FORMAT(NOW(), '%H:%i:%s'), signature = SUBSTRING(MD5(RAND()) FROM 1 FOR 32), log_type = " & ReverseIntType() & ", campus_id = " & campus_id, db_raw_data)
            If is_online Then
                ExecQuery("INSERT INTO `" & bioTableName & "` SET emp_id = '" & client_ID & "', log_date = DATE_FORMAT(NOW(), '%Y-%m-%d'), log_time = DATE_FORMAT(NOW(), '%H:%i:%s'), signature = SUBSTRING(MD5(RAND()) FROM 1 FOR 32), log_type = " & ReverseIntType() & ", campus_id = " & campus_id, dtrs_raw_data)
            Else
                ExecQuery("INSERT INTO `unsaved` SET emp_id = '" & client_ID & "', log_date = DATE_FORMAT(NOW(), '%Y-%m-%d'), log_time = DATE_FORMAT(NOW(), '%H:%i:%s'), signature = SUBSTRING(MD5(RAND()) FROM 1 FOR 32), log_type = " & ReverseIntType() & ", campus_id = " & campus_id, db_raw_data)
            End If
        Else
            Exit Sub
        End If
    End Sub

    Function IntType() As Byte
        Dim x As Byte = 1
        Select Case type
            Case "IN"
                x = 1
            Case "OUT"
                x = 2
            Case "OT-IN"
                x = 3
            Case "OT-OUT"
                x = 4
        End Select
        Return x
    End Function

    Function ReverseIntType() As Byte
        Dim x As Byte = 1
        Select Case type
            Case "IN"
                x = 2
            Case "OUT"
                x = 1
            Case "OT-IN"
                x = 4
            Case "OT-OUT"
                x = 3
        End Select
        Return x
    End Function

    Sub ShowData(ID As String)
        If Not dbreader.IsClosed Then dbreader.Close()
        command.Connection = db_master
        command.CommandText = "SELECT CONCAT(FirstName, ' ', LastName), SchedCode FROM `tbl_employee` WHERE EmployeeID = '" & ID & "'"
        dbreader = command.ExecuteReader()
        If dbreader.Read Then
            clientName = dbreader.Item(0).ToString()
            shiftID = dbreader.Item(1).ToString()
        End If
        dbreader.Close()
    End Sub

    Function IsAdmin(id As String) As Boolean
        If Not dbreader.IsClosed Then dbreader.Close()
        command.Connection = db_attendance
        command.CommandText = "SELECT * FROM `tbl_user` WHERE priv_id = 1 AND emp_id = '" & id & "'"
        dbreader = command.ExecuteReader()
        If dbreader.HasRows Then
            dbreader.Close()
            Return True
        End If
        Return False
    End Function

    Function CheckTable(strDate As String) As Boolean
        command.Connection = db_raw_data
        command.CommandText = "SELECT table_name FROM information_schema.tables WHERE table_name = " & bioTableName
        dbreader = command.ExecuteReader()
        If dbreader.HasRows Then
            dbreader.Close()
            Return True
        End If
        Return False
    End Function

    Sub GenerateTable()
        If Not dbreader.IsClosed Then dbreader.Close()
        ExecQuery("CREATE TABLE IF NOT EXISTS `" & bioTableName & "` (log_no INT UNSIGNED NOT NULL PRIMARY KEY AUTO_INCREMENT, emp_id CHAR(20), log_date DATE, log_time TIME, log_type TINYINT UNSIGNED, signature CHAR(50), campus_id TINYINT(3) UNSIGNED)", db_raw_data)
        If CheckConnection(1) Then
            CheckAttendanceTable(bioTableName)
            ExecQuery("CREATE TABLE IF Not EXISTS `" & bioTableName & "` (log_no INT UNSIGNED Not NULL PRIMARY KEY AUTO_INCREMENT, emp_id Char(20), log_date Date, log_time TIME, log_type TINYINT UNSIGNED, signature Char(50), campus_id TINYINT(3) UNSIGNED)", dtrs_raw_data)
        End If
    End Sub

    Public Sub SaveFingerprint(fprint As String)
        _fprint(index) = fprint
    End Sub

    Function ExecQuery(query As String, db As MySqlConnection) As Boolean
        If Not dbreader.IsClosed Then dbreader.Close()
        command = New MySqlCommand(query, db)
        command.ExecuteNonQuery()
        Return True
    End Function

    Public Sub SendRecent()
        SendMessage(Action.SetText, type & "," & clientName & "," & Now.ToShortTimeString)
    End Sub

    Public Sub ShowRecentLog()
        If dgReport.InvokeRequired Then
            dgReport.Invoke(Sub() ShowRecentLog())
            Return
        End If
        dgReport.Rows.Clear()
        If Not dbreader.IsClosed Then dbreader.Close()
        command.Connection = db_attendance
        command.CommandText = "Select log_type, FirstName, LastName, DATE_FORMAT(log_time, '%h:%i%p') FROM `bio_db_raw_data`.`" & bioTableName & "`, bio_db_master.`tbl_employee` WHERE emp_id = EmployeeID AND log_date = '" & Now.ToString("yyyy-MM-d") & "' ORDER BY log_time DESC LIMIT 0, 15"
        dbreader = command.ExecuteReader()
        If dbreader.HasRows Then
            While dbreader.Read
                If dbreader.Item(0) = 1 Then
                    dgReport.Rows.Add("IN", dbreader.Item(1) & " " & dbreader.Item(2), dbreader.Item(3))
                Else
                    dgReport.Rows.Add("OUT", dbreader.Item(1) & " " & dbreader.Item(2), dbreader.Item(3))
                End If
            End While
        End If
        dbreader.Close()
    End Sub

    Sub GetSettings()
        settings.Clear()

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


    Sub ConnectOnlineDatabase()
        If CheckConnection(1) Then
            is_online = True
            dtrs_master = Online_Database("db_master")
            dtrs_raw_data = Online_Database("db_raw_data")
            dtrs_attendance = Online_Database("db_attendance")
        Else
            is_online = False
        End If
    End Sub

    Sub SetID()
        txtID.Text = newEmpId
    End Sub
    Sub UpdateDatabase()
        If is_online Then
            CheckUnsaved()
            If CompareTable("tbl_employee") Then
                If CompareTable("tbl_fingerprint") Then
                    is_updated = True
                End If
            Else
                UpdateDatabase()
            End If
        End If
    End Sub

    Sub CheckUnsaved()
        If Not dbreader.IsClosed Then dbreader.Close()
        command.Connection = db_raw_data
        command.CommandText = "SELECT DATE_FORMAT(log_date, '%m-%Y') as tbl,emp_id,DATE_FORMAT(log_date, '%Y-%m-%d'),log_time,log_type,signature,campus_id FROM unsaved"
        dbreader = command.ExecuteReader()
        If dbreader.HasRows Then
            While dbreader.Read
                Dim query = "INSERT INTO `" & dbreader(0).ToString & "` (`emp_id`,`log_date`,`log_time`,`log_type`,`signature`,`campus_id`) SELECT * FROM (SELECT '" & dbreader(1).ToString & "','" & dbreader(2).ToString & "','" & dbreader(3).ToString & "','" & dbreader(4).ToString & "','" & dbreader(5).ToString & "','" & dbreader(6).ToString & "') AS tmp WHERE NOT EXISTS (SELECT * FROM `" & dbreader(0) & "` WHERE signature = '" & dbreader(5) & "')"
                command = New MySqlCommand(query, dtrs_raw_data)
                command.ExecuteNonQuery()
            End While
            dbreader.Close()
            DeleteSaved()
        End If
    End Sub

    Sub DeleteSaved()
        ExecQuery("DELETE FROM unsaved WHERE 1", db_raw_data)
    End Sub

    Function CompareTable(tbl As String) As Boolean
        Dim online, offline As Integer
        online = CountElement(dtrs_master, tbl)
        offline = CountElement(db_master, tbl)
        If online > offline Then
            If tbl = "tbl_fingerprint" Then
                UpdateFingerprint(dtrs_master, db_master)
                LoadFingerprints()
            ElseIf tbl = "tbl_employee" Then
                UpdateEmployee(dtrs_master, db_master)
                Return False
            End If
        Else
            Return True
        End If
        If online < offline Then
            If tbl = "tbl_fingerprint" Then
                UpdateFingerprint(db_master, dtrs_master)
            End If
        End If
    End Function

    Function CountElement(db As MySqlConnection, tbl As String) As Integer
        If Not dbreader.IsClosed Then dbreader.Close()
        Dim count As Integer

        command.Connection = db
        command.CommandText = "SELECT COUNT(*) FROM `" & tbl & "` WHERE 1"
        dbreader = command.ExecuteReader()
        If dbreader.HasRows Then
            While dbreader.Read
                count = dbreader.Item(0)
            End While
        End If
        Return count
        dbreader.Close()
    End Function

    Sub UpdateFingerprint(db1 As MySqlConnection, db2 As MySqlConnection)
        If Not dbreader.IsClosed Then dbreader.Close()
        command.Connection = db1
        command.CommandText = "SELECT * FROM tbl_fingerprint WHERE 1"
        dbreader = command.ExecuteReader()
        If dbreader.HasRows Then
            While dbreader.Read
                Dim query = "INSERT INTO `tbl_fingerprint`(`emp_id`, `xml`) SELECT * FROM (SELECT '" & dbreader(1) & "','" & dbreader(2) & "') AS tmp WHERE NOT EXISTS (SELECT * FROM tbl_fingerprint WHERE emp_id= '" & dbreader(1) & "' AND xml ='" & dbreader(2) & "')"
                command = New MySqlCommand(query, db2)
                command.ExecuteNonQuery()
            End While
        End If
        dbreader.Close()
    End Sub

    Sub UpdateEmployee(db1 As MySqlConnection, db2 As MySqlConnection)
        If Not dbreader.IsClosed Then dbreader.Close()
        command.Connection = db1
        command.CommandText = "SELECT `eID`,`EmployeeID`, `LastName`, `FirstName`, `MiddleName`, `ExtName`, `EmpPicture`, `SchedCode`, `ActiveStatus`, `CampusID` FROM tbl_employee WHERE 1"
        dbreader = command.ExecuteReader()
        If dbreader.HasRows Then
            While dbreader.Read
                Dim query = "INSERT IGNORE INTO `tbl_employee`(`eID`,`EmployeeID`, `LastName`, `FirstName`, `MiddleName`, `ExtName`, `EmpPicture`, `SchedCode`, `ActiveStatus`, `CampusID`) VALUES ('" & dbreader(0) & "','" & dbreader(1) & "','" & dbreader(2) & "','" & dbreader(3) & "','" & dbreader(4) & "','" & dbreader(5) & "','" & dbreader(6) & "','" & dbreader(7) & "','" & dbreader(8) & "','" & dbreader(9) & "')"
                command = New MySqlCommand(query, db2)
                command.ExecuteNonQuery()
            End While
        End If
        dbreader.Close()
    End Sub

    Sub CheckAttendanceTable(period As String)
        Dim isExist As Boolean = False
        Try
            command = New MySqlCommand("SHOW TABLES LIKE '" & period & "'", dtrs_attendance)
            dbreader = command.ExecuteReader

            If dbreader.HasRows Then
                isExist = True
            End If
        Catch ex As Exception
            Console.WriteLine("CheckAttendanceTable(): " & ex.Message)
        Finally
            If Not isExist Then
                If Not dbreader.IsClosed Then dbreader.Close()
                command = New MySqlCommand("CREATE TABLE `" & period & "` (
                      `id` int(10) UNSIGNED NOT NULL,
                      `emp_id` char(20) DEFAULT NULL,
                      `date` char(20) NOT NULL,
                      `am_in` char(20) DEFAULT NULL,
                      `am_out` char(20) DEFAULT NULL,
                      `pm_in` char(20) DEFAULT NULL,
                      `pm_out` char(20) DEFAULT NULL,
                      `ot_in` char(20) DEFAULT NULL,
                      `ot_out` char(20) DEFAULT NULL,
                      `is_absent` enum('0','1') DEFAULT NULL,
                      `total_hours` double(3,2) DEFAULT NULL,
                      `late` smallint(6) DEFAULT NULL,
                      `undertime` smallint(6) NOT NULL,
                      `remarks` char(255) DEFAULT NULL,
                      `status` tinyint(4) NOT NULL,
                      `signature` char(32) DEFAULT NULL
                    ) ENGINE=InnoDB DEFAULT CHARSET=latin1;

                    ALTER TABLE `" & period & "`
                      ADD PRIMARY KEY (`id`),
                      ADD UNIQUE KEY `signature` (`signature`),
                      ADD KEY `emp_id` (`emp_id`);

                    ALTER TABLE `" & period & "`
                      MODIFY `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;
                    
                    ALTER TABLE `" & period & "`
                      ADD CONSTRAINT `" & period & "_ibfk_1` FOREIGN KEY (`emp_id`) REFERENCES `db_master`.`tbl_employee` (`EmployeeID`);", dtrs_attendance)
                command.ExecuteNonQuery()
            End If

        End Try
    End Sub

#Region "Initialize Fingerprint reader"
    Public Sub GetReaders()
        _freaders.Clear()
        Dim _readers = ReaderCollection.GetReaders()
        If _readers.Count > 0 Then
            Dim _fprint As New Fingerprint
            For i = 0 To _readers.Count - 1
                _fprint.CurrentReader = _readers(i)
                If Not _fprint.OpenReader() Then
                    MsgBox("Fingerprint reader error. Please try again.", MsgBoxStyle.Critical, "Reader error")
                    Application.Exit()
                End If
                CaptureReader(_fprint)
                _freaders.Add(_fprint)
            Next
        Else
            MsgBox("Fingerprint reader not found." & vbNewLine & "Please connect the reader and try again.", MsgBoxStyle.Critical, "Reader not found.")
            Application.Exit()
        End If

    End Sub

    Private Sub CaptureReader(x As Fingerprint)
        Try
            If Not x.StartCaptureAsync(AddressOf OnCaptured) Then
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub OnCaptured(ByVal captureResult As CaptureResult)
        Dim _fmdFprint As Fmd = Nothing
        Try
            ' Check capture quality and throw an error if bad.
            For i = 0 To _freaders.Count - 1
                If Not _freaders(i).CheckCaptureResult(captureResult) Then Return
                For Each fiv As Fid.Fiv In captureResult.Data.Views
                    SendMessage(Action.SendBitmap, _freaders(i).CreateBitmap(fiv.RawImage, fiv.Width, fiv.Height))
                Next
                Dim resultConversion As DataResult(Of Fmd) = FeatureExtraction.CreateFmdFromFid(captureResult.Data, Formats.Fmd.ANSI)

                If resultConversion.ResultCode <> ResultCode.DP_SUCCESS Then
                    Throw New Exception("" & resultConversion.ResultCode.ToString())
                End If
                _fmdFprint = resultConversion.Data
            Next
            Verify(_fmdFprint)
        Catch ex As Exception
            MsgBox("ERROR ON OnCaptured Method" & vbNewLine & ex.Message)
        End Try
    End Sub

    Public Enum Action
        SendBitmap
        SendMessage
        SetText
    End Enum

    Private Delegate Sub SendMessageCallback(ByVal action As Action, ByVal payload As Object)
    Public Sub SendMessage(ByVal action As Action, ByVal payload As Object)
        On Error Resume Next
        If frmIndex = 0 Then
            img = New Bitmap(payload, 178, 196)
        Else
            img = New Bitmap(payload, 357, 392)
        End If

        If fpBox.InvokeRequired Then
            Dim d As New SendMessageCallback(AddressOf SendMessage)
            frmForm.Invoke(d, New Object() {action, payload})
        Else
            Select Case action
                Case Action.SendBitmap
                    If frmIndex = 1 And _flag = False Then
                        fpBox.Visible = False
                    Else
                        fpBox.Visible = True
                    End If
                    fpBox.BackgroundImage = img
                    fpBox.Refresh()
                Case Action.SendMessage
                    MessageBox.Show(DirectCast(payload, String))
            End Select
        End If
    End Sub

    Private Delegate Sub SendTextCallback(ByVal action As Action, ByVal payload As String)
    Public Sub SendMessage(ByVal action As Action, ByVal payload As String)
        On Error Resume Next

        If dgReport.InvokeRequired Then
            Dim d As New SendTextCallback(AddressOf SendMessage)
            frmForm.Invoke(d, New Object() {action, payload})
        Else
            Select Case action
                Case Action.SetText
                    Dim data() As String
                    data = payload.Split(",")
                    dgReport.Rows.Insert(0, data(0), data(1), data(2))
                    dgReport.Rows(0).Selected = True
            End Select
        End If
    End Sub
#End Region
End Module
