Imports DPUruNet
Imports DPUruNet.Constants

Module FingerprintModule
    Public _validated As Boolean
    Public CheckCount As Integer = 0
    Public strFingerprint As String
    Private Const PROBABILITY_ONE As Integer = &H7FFFFFFF

    Function Validate(Fingerprint As String, FingerprintY As Fmd) As Boolean
        Dim FingerprintX As Fmd = Fmd.DeserializeXml(Fingerprint)
        Dim compareResult = Comparison.Compare(FingerprintX, 0, FingerprintY, 0)

        If compareResult.ResultCode <> ResultCode.DP_SUCCESS Then
            Return False
        End If

        If compareResult.Score < PROBABILITY_ONE / 100000 Then Return True
        Return False
    End Function

    Function RegValidate(Fingerprint As Fmd) As Boolean
        Dim compareResult = Comparison.Compare(fingerprint1, 0, Fingerprint, 0)

        If compareResult.ResultCode <> ResultCode.DP_SUCCESS Then
            Throw New Exception("" & compareResult.ResultCode.ToString())
        End If
        If compareResult.Score < PROBABILITY_ONE / 100000 Then
            Return _validated = False
        Else
            Return _validated = True
        End If
    End Function
End Module
