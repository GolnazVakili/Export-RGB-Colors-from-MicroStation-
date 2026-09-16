Attribute VB_Name = "ExportAllColorCodes"
Option Explicit

' Export every color code in the attached MicroStation color table (0-255)
' with RGB. Unused table slots are still written; this is not limited to
' colors assigned to a layer.
'
' In MicroStation:
'   1. Utilities > Macros > VBA Manager (or Drawing > Macros)
'   2. Import this .bas into a VBA project
'   3. Key-in:  vba run ExportAllColorCodes
'
' Optional: also append ByLevel rows (ColorIndex -1 + layer name):
'   vba run ExportAllColorCodesAndLevels

Private Sub ExtractRGB(ByVal longColor As Long, ByRef intRed As Byte, ByRef intGreen As Byte, ByRef intBlue As Byte)
    Dim lngColor As Long
    lngColor = longColor
    intRed = lngColor Mod &H100
    lngColor = lngColor \ &H100
    intGreen = lngColor Mod &H100
    lngColor = lngColor \ &H100
    intBlue = lngColor Mod &H100
End Sub

Private Function CsvEscape(ByVal value As String) As String
    If InStr(value, ",") > 0 Or InStr(value, """") > 0 Or InStr(value, vbCr) > 0 Or InStr(value, vbLf) > 0 Then
        CsvEscape = """" & Replace(value, """", """""") & """"
    Else
        CsvEscape = value
    End If
End Function

Private Function PackedColorAt(ByRef col As Variant, ByVal colorIndex As Long) As Long
    Dim lo As Long, hi As Long
    lo = LBound(col)
    hi = UBound(col)

    If colorIndex >= lo And colorIndex <= hi Then
        PackedColorAt = CLng(col(colorIndex))
        Exit Function
    End If

    ' 1-based SAFEARRAY: color n is stored at n (or n+1 when LBound is 1 and
    ' the array is 1..256 for codes 0..255).
    If lo = 1 And (colorIndex + 1) >= lo And (colorIndex + 1) <= hi Then
        PackedColorAt = CLng(col(colorIndex + 1))
        Exit Function
    End If

    PackedColorAt = 0
End Function

Private Function DefaultCsvPath() As String
    DefaultCsvPath = ActiveDesignFile.FullName & "-all-color-codes.csv"
End Function

Private Sub WriteColorTableRows(ByVal fileNum As Integer, ByRef col As Variant)
    Dim i As Long
    Dim r As Byte, g As Byte, b As Byte

    For i = 0 To 255
        ExtractRGB PackedColorAt(col, i), r, g, b
        Print #fileNum, CStr(i) & "," & CsvEscape(CStr(r) & ", " & CStr(g) & ", " & CStr(b)) & "," & CStr(r) & "," & CStr(g) & "," & CStr(b) & ","
    Next
End Sub

Private Sub WriteLevelRows(ByVal fileNum As Integer, ByRef col As Variant)
    Dim lvl As Level
    Dim colorIndex As Long
    Dim r As Byte, g As Byte, b As Byte

    For Each lvl In ActiveDesignFile.Levels
        If lvl Is Nothing Then GoTo NextLevel
        If Len(lvl.Name) = 0 Then GoTo NextLevel

        On Error GoTo NextLevel
        colorIndex = lvl.ElementColor
        On Error GoTo 0

        If colorIndex >= 0 And colorIndex <= 255 Then
            ExtractRGB PackedColorAt(col, colorIndex), r, g, b
        Else
            ExtractRGB colorIndex, r, g, b
        End If

        Print #fileNum, "-1," & CsvEscape(CStr(r) & ", " & CStr(g) & ", " & CStr(b)) & "," & CStr(r) & "," & CStr(g) & "," & CStr(b) & "," & CsvEscape(lvl.Name)
NextLevel:
        On Error GoTo 0
    Next
End Sub

Private Sub ExportColorCodes(ByVal includeLevels As Boolean)
    Dim tbl As ColorTable
    Dim col As Variant
    Dim csvPath As String
    Dim fileNum As Integer

    If ActiveDesignFile Is Nothing Then
        MsgBox "Open a DGN file and try again.", vbExclamation, "Export color codes"
        Exit Sub
    End If

    Set tbl = ActiveDesignFile.ExtractColorTable
    col = tbl.GetColors
    csvPath = DefaultCsvPath()

    fileNum = FreeFile
    Open csvPath For Output As #fileNum
    Print #fileNum, "ColorIndex,RGB,R,G,B,Layer"
    WriteColorTableRows fileNum, col
    If includeLevels Then
        WriteLevelRows fileNum, col
    End If
    Close #fileNum

    MsgBox "Wrote all 256 color-table codes with RGB to:" & vbCrLf & csvPath, vbInformation, "Export color codes"
End Sub

Public Sub ExportAllColorCodes()
    ExportColorCodes includeLevels:=False
End Sub

Public Sub ExportAllColorCodesAndLevels()
    ExportColorCodes includeLevels:=True
End Sub
