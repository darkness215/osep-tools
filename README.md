# OSEP Toolkit

A collection of offensive tools I built and used throughout my OSEP journey. The toolkit covers the full attack chain: initial access via Word macros and HTA files, shellcode encryption and delivery, process injection, AppLocker bypass, AMSI evasion, and UAC bypass.

---

## Tools

### `word-macro/`
VBA macros for Microsoft Word. Covers the full progression from basic shellcode runners to AppLocker bypass.

| Folder | Technique |
|---|---|
| `recon/` | Fingerprint the target before choosing a payload — detects AppLocker, OS arch, Office bitness |
| `caesar-cipher/` | Caesar +2 encrypted shellcode runner |
| `xor-shellcode-runner/` | 3-byte rolling XOR runner — stronger than Caesar, W^X memory model |
| `alternative-shellcode-runner/` | XOR runner using `HeapAlloc` + `EnumSystemGeoID` callback instead of `VirtualAlloc` + `CreateThread` |
| `installutil-applocker-bypass/` | Full AppLocker bypass chain — WMI process spawn, bitsadmin download, `installutil.exe /U` |

---

### `aes-loaders/`
C# shellcode runners using AES-256-CBC encryption. Shellcode is encrypted on the attacker side and decrypted in memory at runtime — nothing plaintext touches disk.

| Folder | Technique |
|---|---|
| `aes-shellcode-encryption/` | Encryptor — always start here to generate your encrypted buf |
| `aes-normal-runner/` | Self-injection via `VirtualAlloc` + `CreateThread` |
| `aes-process-injection/` | Remote injection into a target process via `CreateRemoteThread` |
| `aes-av-evasion/` | Remote injection with sandbox evasion — `VirtualAllocExNuma`, timing checks, DLL presence checks |
| `aes-process-hollowing/` | Hollows `svchost.exe` via suspended process + PEB walk — no `CreateRemoteThread` |

---

### `amsi-bypass/`
PowerShell AMSI bypass scripts. The `amsi-bypass.ps1` patches `AmsiOpenSession` directly in memory — more reliable than the `amsiInitFailed` reflection trick which gets caught regularly. `amsi-techniques.md` documents all three approaches with code.

---

### `hta-delivery/`
HTA file executed by `mshta.exe`. Downloads a base64-encoded payload via `bitsadmin`, decodes it with `certutil`, and runs it through `installutil.exe /U`. A clean alternative to Word macros when Office isn't available on the target.

---

### `ps-loaders/`
Two PowerShell in-memory execution tools.

| File | Purpose |
|---|---|
| `ps-loader.ps1` | Kills AMSI, ETW, and script block logging before downloading and executing a payload over HTTP — nothing touches disk |
| `ps-shellcode-runner.ps1` | Reflection-based shellcode runner — resolves `VirtualAlloc`, `CreateThread`, `WaitForSingleObject` entirely via `LookupFunc` at runtime, no P/Invoke declarations |

---

### `uac-bypass/`
`fodhelper.exe` registry hijack. Writes the payload command into `HKCU\Software\Classes\ms-settings\shell\open\command`, launches `fodhelper.exe`, and cleans up. The `uac-bypass.ps1` has AMSI bypass baked in and builds all sensitive strings from char arrays to avoid static detection. Manual one-liners are also documented for when you already have a shell and just need quick elevation.

---

## Requirements

| Tool | Used for |
|---|---|
| `msfvenom` / Sliver | Generate shellcode |
| Python 3 | Encryption scripts, HTTP server |
| .NET SDK | Compile C# runners |
| Visual Studio / VS Code | Build C# projects (Release, x64) |

