
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports MySql.Data.MySqlClient

Public Class Attendance
    Private _empid As New List(Of String)
    Private data(3) As String
    Private db(9) As MySqlConnection
    Private x, y As Byte
    Private localdb() As String = {"localhost", "root", ""}
    Private stmt As String

    Private cmd As MySqlCommand
    Private rdr As MySqlDataReader

    Public Sub Start(period As String, month As String, year As String)
        data(0) = period
        data(1) = month
        data(2) = year

        If is_online Then
            db(7) = New MySqlConnection("server=" & settings("online_server_ip") & "; port=3306; user id=" & settings("online_server_user") & ";password=" & settings("online_server_password"))
            Try
                db(7).Open()
            Catch ex As Exception
                Console.WriteLine("Unable to open online database.")
            End Try
        End If

        Initialize()
    End Sub

    Private Sub Initialize()
        'Init Databases
        db(0) = LocalDatabase("bio_db_master")
        db(1) = LocalDatabase("bio_db_raw_data")
        db(2) = LocalDatabase("bio_db_attendance")

        GetEmployees()
        Period()
        CheckTable(data(1) & "-" & data(2))

        For Each i In _empid
            For j = x To y
                Attendance(i, data(2) & "-" & data(1) & "-" & j)
            Next
        Next
    End Sub

    Private Sub Period()
        If data(0) = "1" Then
            x = 1
            y = 15
        ElseIf data(0) = "2" Then
            x = 16
            y = 31
        Else
            x = 1
            y = 31
        End If
    End Sub

    Sub GetEmployees()
        Try
            db(0).Open()

            cmd = New MySqlCommand("SELECT EmployeeID FROM tbl_employee WHERE ActiveStatus = 1", db(0))
            rdr = cmd.ExecuteReader


            If rdr.HasRows Then
                _empid.Clear()
                While rdr.Read
                    _empid.Add(rdr.Item("EmployeeID"))
                End While
            End If
        Catch ex As Exception
            Console.WriteLine("GetEmployees() ERROR: " & ex.Message)
        Finally
            If Not db(0).State = ConnectionState.Closed Then db(0).Close()
            If Not rdr.IsClosed Then rdr.Close()
        End Try
    End Sub

    Sub Attendance(id As String, logdate As String)
        Dim logs = GetLogs(id, logdate)

        If logs.Rows.Count > 0 Then

            If IsComplete(logs) Then
                If logs.Rows.Count = 4 Then
                    PostAttendance(1, id, logdate, logs)
                Else
                    PostAttendance(2, id, logdate, logs)
                End If
            Else
                If logs.Rows.Count = 4 Then
                    PostAttendance(1, id, logdate, logs)
                Else
                    PostAttendance(2, id, logdate, logs)
                End If
            End If
        Else
            InsertBlank(id, logdate)
        End If

    End Sub

    Private Sub InsertBlank(id As String, logdate As String)
        Dim log() As String
        Try
            If CDate(logdate).ToString("dddd") = "Saturday" Then
                log = {" S ", " - ", " A ", " - ", " T ", " - "}
            ElseIf CDate(logdate).ToString("dddd") = "Sunday" Then
                log = {" S ", " - ", " U ", " - ", " N ", " - "}
            Else
                log = {" : ", " : ", " : ", " : ", "", ""}
            End If
            If Not CheckAttendanceCount(id, logdate) Then
                ExecQuery("INSERT INTO `" & data(1) & "-" & data(2) & "` SET emp_id = '" & id & "', date = '" & logdate & "', am_in = '" & log(0) & "', am_out = '" & log(1) & "', pm_in = '" & log(2) & "', pm_out = '" & log(3) & "', ot_in = '" & log(4) & "', ot_out = '" & log(5) & "', signature = SUBSTRING(MD5(RAND()) FROM 1 FOR 32)", db(2))
            End If
        Catch ex As Exception

        End Try
    End Sub



    Function CheckAttendanceCount(id As String, logdate As String) As Boolean
        If Not dbreader.IsClosed Then dbreader.Close()
        If Not dtrs_attendance.State = ConnectionState.Open Then dtrs_attendance.Open()
        command.Connection = dtrs_attendance
        command.CommandText = "SELECT * FROM `" & data(1) & "-" & data(2) & "` WHERE emp_id = '" & id & "' AND date = '" & logdate & "'"
        dbreader = command.ExecuteReader()
        If dbreader.HasRows Then
            dbreader.Close()
            Return True
        End If
        dbreader.Close()
        Return False
    End Function

    Private Sub PostAttendance(opt As Byte, id As String, logdate As String, logs As DataTable)
        Dim log(logs.Rows.Count - 1) As String
        Dim hours(5) As Double

        For i = 0 To logs.Rows.Count - 1
            log(i) = logs(i)("log_time")
        Next

        logdate = CDate(logdate).ToString("yyyy-MM-dd")
        If opt = 1 Then
            log = CleanLogs(log)
            hours = CalculateHours(id, log, logdate)
        Else
            log = CheckLogs(logs)
            hours = CalculateHours(id, log, logdate)
        End If
        Dim sig As String = Hash(BuildString(log, logdate, id))
        If CheckAttendanceCount(id, logdate) Then
            stmt = "UPDATE `" & data(1) & "-" & data(2) & "` SET am_in = '" & log(0) & "', am_out = '" & log(1) & "', pm_in = '" & log(2) & "', pm_out = '" & log(3) & "', ot_in = '" & log(4) & "', ot_out = '" & log(5) & "', total_hours = " & hours(0) & ", late = " & hours(1) & ", undertime = " & hours(2) & ", signature = '" & sig & "' WHERE emp_id = '" & id & "' AND date = '" & CDate(logdate).ToString("yyyy-MM-dd") & "'"
        Else
            stmt = "INSERT INTO  `" & data(1) & "-" & data(2) & "` SET emp_id = '" & id & "', date = '" & CDate(logdate).ToString("yyyy-MM-dd") & "', am_in = '" & log(0) & "', am_out = '" & log(1) & "', pm_in = '" & log(2) & "', pm_out = '" & log(3) & "', ot_in = '" & log(4) & "', ot_out = '" & log(5) & "', total_hours = " & hours(0) & ", late = " & hours(1) & ", undertime = " & hours(2) & ", signature = '" & sig & "'"
        End If
        ExecQuery(stmt, db(2))

        'If is_online Then
        'ExecQuery(stmt, db(7))
        'End If
    End Sub

    Private Sub DoTask() Handles task.DoWork
        UploadAttendance("a=attendance&stmt=" & stmt)
    End Sub

    Private Sub UploadAttendance(stmt As String)
        Dim url As String = "https://dtrs.cbsuainfotech.com"
        Console.WriteLine(stmt)
        Try
            Dim request As Net.WebRequest = Net.WebRequest.Create(url)
            request.Method = "POST"
            Dim postData = stmt
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

    Private Function BuildString(log() As String, logdate As String, id As String) As String
        Dim result As String = logdate & id
        For Each a In log
            result &= a
        Next
        Return result
    End Function

    Private Function Hash(input As String) As String
        Using hasher As MD5 = MD5.Create()    ' create hash object
            Dim dbytes As Byte() = hasher.ComputeHash(Encoding.UTF8.GetBytes(input))
            Return Convert.ToBase64String(dbytes)
        End Using
    End Function

    Private Function CalculateHours(id As String, logs() As String, logdate As Date) As Double()
        Dim total(5) As Double
        Dim schedule As DataTable = GetSchedule(id)

        total(0) = 0 ' total hours
        total(1) = 0 ' late
        total(2) = 0 ' undertime

        For i = 0 To schedule.Rows.Count - 1
            If schedule(i)("weekday") = logdate.ToString("dddd") Then
                ' MORNING LOG
                If logs(0) <> ":" AndAlso logs(1) <> ":" Then
                    ' MORNING LATE
                    If Date.Compare(logs(0), schedule(i)("am_in")) > 0 Then
                        total(1) += (CDate(logs(0)) - CDate(schedule(i)("am_in"))).TotalMinutes
                    End If
                    ' MORNING UNDERTIME
                    If Date.Compare(logs(1), schedule(i)("am_out")) < 0 Then
                        total(2) += (CDate(schedule(i)("am_out")) - CDate(logs(1))).TotalMinutes
                    End If
                Else
                    total(2) += (CDate(schedule(i)("am_out")) - CDate(schedule(i)("am_in"))).TotalMinutes
                End If
                total(0) += (CDate(schedule(i)("am_out")) - CDate(schedule(i)("am_in"))).TotalMinutes

                ' AFTERNOON LOG
                If logs(2) <> ":" AndAlso logs(3) <> ":" Then
                    ' MORNING LATE
                    If Date.Compare(logs(2), schedule(i)("pm_in")) > 0 Then
                        total(1) += (CDate(logs(2)) - CDate(schedule(i)("pm_in"))).TotalMinutes
                    End If
                    ' MORNING UNDERTIME
                    If Date.Compare(logs(3), schedule(i)("pm_out")) < 0 Then
                        total(2) += (CDate(schedule(i)("pm_out")) - CDate(logs(3))).TotalMinutes
                    End If
                Else
                    total(2) += (CDate(schedule(i)("pm_out")) - CDate(schedule(i)("pm_in"))).TotalMinutes
                End If
                total(0) += (CDate(schedule(i)("pm_out")) - CDate(schedule(i)("pm_in"))).TotalMinutes
            End If
        Next
        total(0) = (total(0) - (total(1) + total(2))) / 60
        Return total
    End Function

    Private Function CheckLogs(logs As DataTable) As String()
        Dim flog() As String = {":", ":", ":", ":", "", ""}
        For i = 0 To logs.Rows.Count - 1
            Dim logtime = CDate(logs(i)("log_time")).ToString("h:mmtt")

            Select Case logs(i)("log_type")
                Case 1
                    If Date.Compare(logtime, "12:00PM") < 0 AndAlso flog(0) = ":" Then
                        flog(0) = logtime
                    Else
                        flog(2) = logtime
                    End If
                Case 2
                    If Date.Compare(logtime, "01:00PM") < 0 AndAlso flog(1) = ":" Then
                        flog(1) = logtime
                    Else
                        flog(3) = logtime
                    End If
                Case 3

                Case 4

            End Select
        Next
        Return flog
    End Function

    Private Function CleanLogs(logs() As String) As String()
        Dim flog() As String = {":", ":", ":", ":", "", ""}
        For i = 0 To logs.Count - 1
            flog(i) = CDate(logs(i)).ToString("h:mmtt")
        Next
        Return flog
    End Function

    Private Function IsComplete(logs As DataTable) As Boolean
        Dim seq() As String = {1, 2, 1, 2, 1, 2}
        For i = 0 To logs.Rows.Count - 1
            If logs(i)("log_type") <> seq(i) Then
                Return False
            End If
        Next
        Return True
    End Function

    Private Function DT(col() As String) As DataTable
        Dim dtable As New DataTable

        For Each a In col
            dtable.Columns.Add(a)
        Next
        Return dtable
    End Function

    Private Function GetSchedule(id As String) As DataTable
        Dim col() As String = {"weekday", "am_in", "am_out", "pm_in", "pm_out"}
        Dim result As DataTable = DT(col)

        Try
            If Not db(0).State = ConnectionState.Open Then db(0).Open()
            cmd = New MySqlCommand("SELECT WeekDay, am_in, am_out, pm_in, pm_out FROM tbl_schedule, tbl_employee WHERE EmployeeID = '" & id & "' AND tbl_schedule.SchedCode = tbl_employee.SchedCode", db(0))
            rdr = cmd.ExecuteReader

            If rdr.HasRows Then
                While rdr.Read
                    result.Rows.Add(rdr.Item(0), rdr.Item(1), rdr.Item(2), rdr.Item(3), rdr.Item(4))
                End While
            End If
        Catch ex As Exception
            Console.WriteLine("GetLogs() ERROR: " & ex.Message)
        Finally
            If Not db(0).State = ConnectionState.Closed Then db(0).Close()
            If Not rdr.IsClosed Then rdr.Close()
        End Try
        Return result
    End Function

    Private Function GetLogs(id As String, logdate As String) As DataTable
        Dim col() As String = {"log_date", "log_time", "log_type"}
        Dim result As DataTable = DT(col)

        Try
            If Not db(1).State = ConnectionState.Open Then db(1).Open()
            cmd = New MySqlCommand("SELECT log_date, log_time, log_type FROM `" & data(1) & "-" & data(2) & "` WHERE log_date = '" & logdate & "' AND emp_id = '" & id & "' ORDER BY log_time ASC", db(1))
            rdr = cmd.ExecuteReader

            If rdr.HasRows Then
                While rdr.Read
                    result.Rows.Add(rdr.Item(0), rdr.Item(1), rdr.Item(2))
                End While
            End If
        Catch ex As Exception
            Console.WriteLine("GetLogs() ERROR: " & ex.Message)
        Finally
            If Not db(1).State = ConnectionState.Closed Then db(1).Close()
            If Not rdr.IsClosed Then rdr.Close()
        End Try
        Return result
    End Function

    Private Sub ExecQuery(query As String, db As MySqlConnection)
        If Not db.State = ConnectionState.Open Then db.Open()
        cmd = New MySqlCommand(query, db)
        cmd.ExecuteNonQuery()
    End Sub

    Private Function LocalDatabase(dbname As String) As MySqlConnection
        Return New MySqlConnection("server = " & localdb(0) & "; user = " & localdb(1) & "; password = " & localdb(2) & "; database = " & dbname)
    End Function

    Private Sub CheckTable(period As String)
        Dim isExist As Boolean = False
        Try
            db(2).Open()

            cmd = New MySqlCommand("SHOW TABLES LIKE '" & period & "'", db(2))
            rdr = cmd.ExecuteReader

            If rdr.HasRows Then
                isExist = True
            End If
        Catch ex As Exception
            Console.WriteLine("CheckTable(): " & ex.Message)
        Finally
            If Not isExist Then
                If Not rdr.IsClosed Then rdr.Close()
                cmd = New MySqlCommand("CREATE TABLE `" & period & "` (
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
                      ADD CONSTRAINT `" & period & "_ibfk_1` FOREIGN KEY (`emp_id`) REFERENCES `bio_db_master`.`tbl_employee` (`EmployeeID`);", db(2))
                cmd.ExecuteNonQuery()
            End If

            If Not db(2).State = ConnectionState.Closed Then db(2).Close()
        End Try
    End Sub

    Friend WithEvents task As ComponentModel.BackgroundWorker
End Class