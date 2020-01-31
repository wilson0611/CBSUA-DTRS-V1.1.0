Imports DPUruNet
Imports MySql.Data.MySqlClient

Module Variables
    'MYSQL RELATED VARIABLES
    Public dbHost = "localhost", dbUser = "root", dbPass = "", dbName = ""
    Public command As New MySqlCommand
    Public dbreader As MySqlDataReader
    Public adapter As MySqlDataAdapter
    Public conn As New MySqlConnection

    'DATABASES
    Public db_attendance, db_raw_data, db_master, dtrs_master, dtrs_raw_data, dtrs_attendance As MySqlConnection
    Public db(10) As MySqlConnection

    'OBJECTS
    Public frmForm As Form = frmMain

    'VARIABLES
    Public att As New Attendance
    Public is_updated As Boolean = False
    Public fingerprint1, _fmdFprint As Fmd
    Public _freaders As New List(Of Fingerprint)
    Public fpBox As PictureBox
    Public txtID As TextBox
    Public frmIndex As Byte = 0
    Public _enable As Boolean
    Public _show As Boolean = False
    Public strFPrint1 As String
    Public strFPrint2 As String
    Public _checkcount As Integer
    Public strNotif As String
    Public dgReport As DataGridView
    Public _success As Boolean
    Public _flag As Boolean = False
    Public campus_id As Byte = 4
    Public is_online As Boolean = False

    Public settings As New Dictionary(Of String, String)

    'TIME IN/OUT VARIABLES
    Public shiftID As String
    Public am_in As String
    Public am_out As String
    Public pm_in As String
    Public pm_out As String
    Public ot_in As String
    Public ot_out As String
    Public shift_am_in As String
    Public shift_am_out As String
    Public shift_pm_in As String
    Public shift_pm_out As String
    Public strText As String = vbNullString
    Public strMsg As String = vbNullString
    Public strError As String = "Please enter employee number."
    Public type As String = "IN"

    Public Count As Byte = 0
    Public btnActive As Byte = 1

    'multi-dimensional array
    Public newEmpId As String
    Public empCount As Integer
    Public fingerprint(empCount, 3) As String

    Public txtGreeting As String
    Public hasTable As Boolean
    Public bioTableName As String = Format(Now, "MM-yyyy")
    Public hasEqual As Boolean
    Public hasclient As Single = -1
    Public client_ID As String
    Public clientName As String
    Public counter As Integer
    Public is_admin As Boolean = False
    Public verify_admin As Boolean = False
    Public img As Bitmap

    'REGISTRATION RELATED VARIABLES
    Public _fprint(2) As String
    Public index As Byte = 0

    Structure FingerPrints
        Dim emp_id As String
        Dim fprint As String
    End Structure

    Public _fingerprints As New List(Of FingerPrints)
End Module
