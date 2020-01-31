Imports MySql.Data.MySqlClient

Module BackgroundTask
    Private table_name As String = Now.ToString("MM") & "-" & Now.ToString("yyyy")
    Private emp_id As New List(Of String)

    Private logs, type As New List(Of String)
    Private lgid As New List(Of Integer)

    Private Sub ClearList()
        logs.Clear()
        type.Clear()
        lgid.Clear()
    End Sub

    Private Sub GetLogs(period As String)
        ClearList()

        For Each x In emp_id
            If Not dbreader.IsClosed Then dbreader.Close()
            command = New MySqlCommand("SELECT log_time, log_type FROM `" & table_name & "` WHERE emp_id = '" & x & "' AND log_date = '" & period & "' ORDER BY emp_id ASC", dtr)
            dbreader = command.ExecuteReader()
            If dbreader.HasRows Then
                While dbreader.Read
                    If dbreader.Item(0).ToString() <> "" Then
                        Dim xtime As Date = dbreader.Item(0).ToString()
                        logs.Add(xtime.ToString("t"))
                    Else
                        logs.Add(dbreader.Item(0).ToString())
                    End If

                    type.Add(dbreader.Item(1).ToString())
                End While
                dbreader.Close()

                If logs.Count > 0 AndAlso type.Count > 0 Then
                    DoAttendance(x, period)
                Else
                    InsertBlank(x, period)
                End If
            Else
                InsertBlank(x, period)
            End If
            ClearList()
        Next
    End Sub

    Sub InsertBlank(id As String, period As String)
        Dim tmpDate As Date = period
        If tmpDate.DayOfWeek.ToString().ToUpper = "SATURDAY" Then
            command = New MySqlCommand("INSERT INTO `" & table_name & "` SET am_in = ' S ', am_out = ' - ', pm_in = ' A ', pm_out = ' - ', ot_in = ' T ', ot_out = ' - ', emp_id = '" & id & "', date = '" & period & "'", attendance)
            command.ExecuteNonQuery()
        ElseIf tmpDate.DayOfWeek.ToString().ToUpper = "SUNDAY" Then
            command = New MySqlCommand("INSERT INTO `" & table_name & "` SET am_in = ' S ', am_out = ' - ', pm_in = ' U ', pm_out = ' - ', ot_in = ' N ', ot_out = ' - ', emp_id = '" & id & "', date = '" & period & "'", attendance)
            command.ExecuteNonQuery()
        Else
            command = New MySqlCommand("INSERT INTO `" & table_name & "` SET am_in = ' : ', am_out = ' : ', pm_in = ' : ', pm_out = ' : ', emp_id = '" & id & "', date = '" & period & "'", attendance)
            command.ExecuteNonQuery()
        End If
    End Sub

    Sub PostAttendance(period As String)
        table_name = period.Substring(period.IndexOf("-") + 1, 2) & "-" & period.Substring(0, 4)
        GetEmployeeID()
        GetLogs(period)
    End Sub

    Sub DoAttendance(id As String, period As String)
        Dim f_logs
        If IsComplete() Then
            f_logs = FillLog()
            command = New MySqlCommand("INSERT INTO `" & table_name & "` SET am_in = '" & f_logs(0) & "', am_out = '" & f_logs(1) & "', pm_in = '" & f_logs(2) & "', pm_out = '" & f_logs(3) & "', ot_in = '" & f_logs(4) & "', ot_out = '" & f_logs(5) & "', emp_id = '" & id & "', date = '" & period & "'", attendance)
            command.ExecuteNonQuery()
        Else
            Dim x As Byte = 1
            Dim tmpLogs() As String = {"  :  ", "  :  ", "  :  ", "  :  ", "    ", "    "}
            For i = 0 To logs.Count - 1
                If type(i) = x Then
                    tmpLogs(i) = logs(i)
                    If x = 1 Then
                        x = 2
                    Else
                        x = 1
                    End If
                End If
            Next

            command = New MySqlCommand("INSERT INTO `" & table_name & "` SET am_in = '" & tmpLogs(0) & "', am_out = '" & tmpLogs(1) & "', pm_in = '" & tmpLogs(2) & "', pm_out = '" & tmpLogs(3) & "', ot_in = '" & tmpLogs(4) & "', ot_out = '" & tmpLogs(5) & "', emp_id = '" & id & "', date = '" & period & "'", attendance)
            command.ExecuteNonQuery()
        End If
    End Sub

    Function FillLog() As String()
        Dim f_logs() As String = {" : ", " : ", " : ", " : ", "   ", "   "}

        For i = 0 To logs.Count - 1
            f_logs(i) = logs(i)
        Next

        Return f_logs
    End Function

    Private Sub CleanLogs()
        Dim seq() As Byte = {1, 2, 1, 2, 1, 2}
        If logs.Count > 0 Then
            If Not IsComplete() Then
                Dim x As Byte = logs.Count

                If logs.Count Mod 2 = 0 Then
                    For i = 0 To logs.Count - 1
                        If Not type(i) = seq(i) Then
                            type(i) = seq(i)
                        End If
                    Next
                Else

                End If
            End If
        End If
    End Sub

    Private Function IsComplete() As Boolean
        Dim seq() As Byte = {1, 2, 1, 2, 1, 2}
        For i = 0 To logs.Count - 1
            If Not type(i) = seq(i) Then
                Return False
            End If
        Next
        Return True
    End Function

    Private Sub GetEmployeeID()
        emp_id.Clear()

        If Not dbreader.IsClosed Then dbreader.Close()
        command = New MySqlCommand("SELECT ReaderID FROM tbl_employee WHERE ActiveStatus = 1", master)
        dbreader = command.ExecuteReader()
        While dbreader.Read
            emp_id.Add(dbreader.Item(0))
        End While
        dbreader.Close()
        MsgBox(emp_id.Count)
    End Sub
End Module
