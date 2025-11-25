using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Agent.Communication
{
    public class Encryptor
    {
        private string secretKey;
        private RijndaelManaged rijndael;
        public Encryptor(string secretKey)
        {
            this.secretKey = secretKey;

            var keyBytes = System.Text.Encoding.UTF8.GetBytes(secretKey);

            byte[] key = keyBytes.Take(32).ToArray();
            byte[] iv = keyBytes.Take(16).ToArray();

            rijndael = new RijndaelManaged();
            rijndael.KeySize = 256;
            rijndael.BlockSize = 128;
            rijndael.Key = key;
            rijndael.IV = iv;
            rijndael.Padding = PaddingMode.PKCS7;
        }

        public byte[] Encrypt(byte[] src)
        {
            return rijndael.CreateEncryptor().TransformFinalBlock(src, 0, src.Length);
        }

        public byte[] Encrypt(string src)
        {
            return Encrypt(System.Text.Encoding.UTF8.GetBytes(src));
        }

       

        public byte[] Decrypt(byte[] src)
        {
            return rijndael.CreateDecryptor().TransformFinalBlock(src, 0, src.Length);
        }

        public string DecryptAsString(byte[] src)
        {
            return System.Text.Encoding.UTF8.GetString(Decrypt(src));
        }

        public byte[] Decrypt(string src)
        {
            return Decrypt(System.Text.Encoding.UTF8.GetBytes(src));
        }

        public string DecryptAsString(string src)
        {
            return DecryptAsString(System.Text.Encoding.UTF8.GetBytes(src));
        }

        public byte[] DecryptFromBase64(string src)
        {
            return this.Decrypt(Convert.FromBase64String(src));
        }

        public string EncryptAsBase64(byte[] src)
        {
            return Convert.ToBase64String(this.Encrypt(src));
        }
    }
}
