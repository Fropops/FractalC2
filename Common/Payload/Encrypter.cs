using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Common.Payload
{
    internal class Encrypter
    {
        public Encrypter()
        {

        }

        public EncryptResult Encrypt(byte[] bytes)
        {
            var secretKey = this.GenerateSecret(48);
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);

            byte[] key = keyBytes.Take(32).ToArray();
            byte[] iv = keyBytes.Take(16).ToArray();

            RijndaelManaged rijndael = new RijndaelManaged();
            rijndael.KeySize = 256;
            rijndael.BlockSize = 128;
            rijndael.Key = key;
            rijndael.IV = iv;
            rijndael.Padding = PaddingMode.PKCS7;

            var encryptedBytes = rijndael.CreateEncryptor().TransformFinalBlock(bytes, 0, bytes.Length);

            EncryptResult result = new EncryptResult(secretKey, encryptedBytes);
            return result;
        }

        private string GenerateSecret(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                builder.Append(chars[random.Next(chars.Length)]);
            }

            string randomString = builder.ToString();
            return randomString;
        }
    }

    internal class EncryptResult
    {
        public string Secret { get; private set; }

        public byte[] Encrypted { get; private set; }

        public EncryptResult(string secret, byte[] encrypted)
        {
            this.Secret = secret;
            this.Encrypted = encrypted;
        }
    }
}
