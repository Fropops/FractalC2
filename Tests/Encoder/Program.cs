using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Encoder
{
    internal class Program
    {
        private static RijndaelManaged encoder;
        private static void InitDecoder()
        {
            var secretKey = "ULxCMoYpwW8KYfjZUnO6Fp6iHIADCQFsBdLVpqdFOVRjCmDr8Gh4RV0yCdLiRKQsbIgsmYiEpAqwIiGKbKLLNFMaCZQiOJBULe9tEGfSc3c4zxYN3xbm7W8iFtgg5agnGEvclMjBzzm1JUy4ATQrgV9kLVj8mNyc8pRHTkrTZybfmJePR46A5e1lwM4Oa9VYkUvE9h4EM2hWjwOPluljn7oOIhZrNoOzpZTdEFaU6RuT7jM1nZXDXcskxKlHPRxE27OqIINJYAR37AT1bCpM0CEoBgBsnCfD24XLRwdUVPWcmaAioH7SiqGgSvXQQGJABB3C6PLI13Uk8NX6PS2JYPO4QVG9PfkjaDSVYb8HacbkDGqg7UdUE7FpSkZ4rM2AeG2XJwniqjZRSVhV0P6ee1OxCCfsOxFR4P3BwPhbTk2ebumgS4dbJzvR9d8hzLTDMIbZmzP9Zubdn2VsLCzsMjmzNLbiyTBDoWuaozkQRl7c6bziL9MlxvOpXkmRyryz";
            var bytes = Encoding.UTF8.GetBytes(secretKey);

            byte[] key = bytes.Take(32).ToArray();
            byte[] iv = bytes.Take(16).ToArray();

            RijndaelManaged rijndael = new RijndaelManaged();
            rijndael.KeySize = 256;
            rijndael.BlockSize = 128;
            rijndael.Key = key;
            rijndael.IV = iv;
            rijndael.Padding = PaddingMode.PKCS7;

            encoder = rijndael;
        }

        private static byte[] Encrypt(byte[] src)
        {
            var encryptedBytes = encoder.CreateEncryptor().TransformFinalBlock(src, 0, src.Length);
            return encryptedBytes;
        }

        static void Main(string[] args)
        {
            int length = 48;
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                builder.Append(chars[random.Next(chars.Length)]);
            }

            string randomString = builder.ToString();
            Console.WriteLine(randomString);

            Console.ReadKey();
            //InitDecoder();
            
            //Encode(@"E:\Share\Projects\C2Sharp\Agent\bin\Debug\Agent.exe", @"E:\Share\Projects\C2Sharp\Payload\agent.enc");

            //Encode(@"E:\Share\Projects\C2Sharp\Payload\PatcherDll\bin\Debug\Patcherdll.dll", @"E:\Share\Projects\C2Sharp\Payload\patcher.enc");
        }

        static void Encode(string i, string o)
        {
            if (File.Exists(o))
                File.Delete(o);


            var bytes = Encrypt(File.ReadAllBytes(i));
            string b64 = Convert.ToBase64String(bytes);


            File.WriteAllText(o, b64);
        }
    }
}
