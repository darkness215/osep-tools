# Word Macro Shellcode Runners

| Folder | Technique |
|---|---|
| `recon/` | Fingerprint target — detect AppLocker, OS arch, Office bitness |
| `caesar-cipher/` | Caesar +2 encrypted shellcode runner |
| `xor-shellcode-runner/` | 3-byte XOR runner via VirtualAlloc + CreateThread |
| `alternative-shellcode-runner/` | 3-byte XOR runner via HeapAlloc + EnumSystemGeoID |
| `installutil-applocker-bypass/` | AppLocker bypass via InstallUtil LOLBin + WMI |

Run `recon/` first to know which technique to deploy.
