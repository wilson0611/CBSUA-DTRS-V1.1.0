Imports System.Threading
Imports DPUruNet
Imports DPUruNet.Constants
Imports System.Drawing.Imaging

Public Class Fingerprint
    Public Property Fmds() As Dictionary(Of Int16, Fmd)
        Get
            Return _fmds
        End Get
        Set(ByVal value As Dictionary(Of Int16, Fmd))
            _fmds = value
        End Set
    End Property
    Private _fmds As Dictionary(Of Int16, Fmd) = New Dictionary(Of Int16, Fmd)

    Public Property Reset() As Boolean
        Get
            Return _reset
        End Get
        Set(ByVal value As Boolean)
            _reset = value
        End Set
    End Property
    Private _reset As Boolean

    ' When set by child forms, shows s/n and enables buttons.
    Public Property CurrentReader() As Reader
        Get
            Return _currentReader
        End Get
        Set(ByVal value As Reader)
            _currentReader = value
        End Set
    End Property
    Private _currentReader As Reader

    Public Function OpenReader() As Boolean
        Try
            Dim result As Constants.ResultCode = Constants.ResultCode.DP_DEVICE_FAILURE

            _reset = False
            result = _currentReader.Open(Constants.CapturePriority.DP_PRIORITY_COOPERATIVE)

            If result <> Constants.ResultCode.DP_SUCCESS Then
                'MessageBox.Show("Error:  " & result.ToString())
                _reset = True
                Return False
            End If

            Return True
        Catch ex As Exception
            'MessageBox.Show(ex.Message)
        End Try

    End Function

    Public Function StartCaptureAsync(ByVal OnCaptured As Reader.CaptureCallback) As Boolean
        Try
            AddHandler _currentReader.On_Captured, OnCaptured

            If Not CaptureFingerAsync() Then
                Return False
            End If

            Return True
        Catch ex As Exception
            'MessageBox.Show(ex.Message)
        End Try

    End Function

    Public Sub CancelCaptureAndCloseReader(ByVal OnCaptured As Reader.CaptureCallback)
        If _currentReader IsNot Nothing Then
            CurrentReader.CancelCapture()

            ' Dispose of reader handle and unhook reader events.
            CurrentReader.Dispose()

            If _reset Then
                CurrentReader = Nothing
            End If
        End If

    End Sub

    Public Sub GetStatus()
        Dim result = _currentReader.GetStatus()

        If (result <> ResultCode.DP_SUCCESS) Then
            If CurrentReader IsNot Nothing Then
                _reset = True
                Throw New Exception("" & result.ToString())
            End If
        End If

        If (_currentReader.Status.Status = ReaderStatuses.DP_STATUS_BUSY) Then
            Thread.Sleep(50)
        ElseIf (_currentReader.Status.Status = ReaderStatuses.DP_STATUS_NEED_CALIBRATION) Then
            _currentReader.Calibrate()
        ElseIf (_currentReader.Status.Status <> ReaderStatuses.DP_STATUS_READY) Then
            Throw New Exception("Reader Status - " & CurrentReader.Status.Status.ToString())
        End If

    End Sub

    Public Function CheckCaptureResult(ByVal captureResult As CaptureResult) As Boolean
        If captureResult.Data Is Nothing Or captureResult.ResultCode <> Constants.ResultCode.DP_SUCCESS Then
            If captureResult.ResultCode <> Constants.ResultCode.DP_SUCCESS Then
                _reset = True
                Throw New Exception("" & captureResult.ResultCode.ToString())
            End If

            If captureResult.Quality <> Constants.CaptureQuality.DP_QUALITY_CANCELED Then
                Throw New Exception("Quality - " & captureResult.Quality.ToString())
            End If
            Return False
        End If
        Return True
    End Function

    Public Function CaptureFingerAsync() As Boolean
        Try
            GetStatus()

            Dim captureResult = _currentReader.CaptureAsync(Formats.Fid.ANSI,
                                                   CaptureProcessing.DP_IMG_PROC_DEFAULT,
                                                    _currentReader.Capabilities.Resolutions(0))

            If captureResult <> ResultCode.DP_SUCCESS Then
                _reset = True
                Throw New Exception("" + captureResult.ToString())
            End If

            Return True
        Catch ex As Exception
            'MessageBox.Show("Error:  " & ex.Message)
            Return False
        End Try
    End Function

    Public Function CreateBitmap(ByVal bytes As [Byte](), ByVal width As Integer, ByVal height As Integer) As Bitmap
        Dim rgbBytes As Byte() = New Byte(bytes.Length * 3 - 1) {}

        For i As Integer = 0 To bytes.Length - 1
            rgbBytes((i * 3)) = bytes(i)
            rgbBytes((i * 3) + 1) = bytes(i)
            rgbBytes((i * 3) + 2) = bytes(i)
        Next
        Dim bmp As New Bitmap(width, height, PixelFormat.Format24bppRgb)

        Dim data As BitmapData = bmp.LockBits(New Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.[WriteOnly], PixelFormat.Format24bppRgb)

        For i As Integer = 0 To bmp.Height - 1
            Dim p As New IntPtr(data.Scan0.ToInt64() + data.Stride * i)
            System.Runtime.InteropServices.Marshal.Copy(rgbBytes, i * bmp.Width * 3, p, bmp.Width * 3)
        Next
        bmp.UnlockBits(data)
        Return bmp
    End Function
End Class
