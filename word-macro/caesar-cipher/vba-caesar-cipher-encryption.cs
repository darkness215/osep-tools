using System;
using System.IO;
using System.Text;

namespace Encrypter
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] buf;

            if (args.Length == 0)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("  Encrypter.exe <shellcode.txt | raw bytes>");
                Console.WriteLine("Example (file): Encrypter.exe shellcode.txt");
                Console.WriteLine("Example (inline): Encrypter.exe 0xfc,0x48,0x83,0xe4,0xf0");
                return;
            }

            // 🔹 Option 1: Read from file if argument is a file
            if (File.Exists(args[0]))
            {
                string fileContent = File.ReadAllText(args[0]).Trim();
                buf = ParseShellcode(fileContent);
            }
            else
            {
                // 🔹 Option 2: Treat argument as inline shellcode string
                buf = ParseShellcode(args[0]);
            }

            // 🔹 Encrypt shellcode
            byte[] encoded = new byte[buf.Length];
            for (int i = 0; i < buf.Length; i++)
            {
                encoded[i] = (byte)(((uint)buf[i] + 2) & 0xFF);
            }

            // 🔹 Print in Visual Basic–style decimal format
            uint counter = 0;
            StringBuilder vbPayload = new StringBuilder(encoded.Length * 4);
            foreach (byte b in encoded)
            {
                vbPayload.AppendFormat("{0:D}, ", b);
                counter++;
                if (counter % 50 == 0)
                {
                    vbPayload.AppendFormat("_{0}", Environment.NewLine);
                }
            }

            // Remove trailing comma and space if needed
            if (vbPayload.Length >= 2)
            {
                vbPayload.Length -= 2;
            }

            Console.WriteLine("Encrypted Payload (VB-style):\n" + vbPayload.ToString());
        }

        // Helper: Parse shellcode string (e.g., "0xfc,0x48,0x83")
        static byte[] ParseShellcode(string input)
        {
            string[] parts = input.Split(new char[] { ',', ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            byte[] result = new byte[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                result[i] = Convert.ToByte(parts[i].Replace("0x", ""), 16);
            }

            return result;
        }
    }
}
