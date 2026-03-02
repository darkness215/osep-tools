# AES Shellcode Loaders

| Folder | Purpose |
|---|---|
| `aes-shellcode-encryption/` | Encryptor — encrypt your shellcode first, always start here |
| `aes-normal-runner/` | Self-injection via VirtualAlloc + CreateThread |
| `aes-process-injection/` | Remote injection into a target process via CreateRemoteThread |
| `aes-av-evasion/` | Remote injection with sandbox evasion checks + VirtualAllocExNuma |
| `aes-process-hollowing/` | Process hollowing into svchost.exe via suspended process + PEB walk |

All runners share the same AES-256-CBC key. Encrypt with `aes-shellcode-encryption/` first, then paste the output into whichever runner you need.
