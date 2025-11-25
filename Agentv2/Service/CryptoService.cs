using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Service
{
    internal interface ICryptoService
    {
        byte[] Key { get; }
        byte[] Encrypt(byte[] data);
        byte[] Decrypt(byte[] data);
    }

    internal class CryptoService : ICryptoService
    {
        public byte[] Key { get; private set; }
        private readonly IConfigService _configService;
        public CryptoService(IConfigService configService)
        {
            _configService = configService;

            this.Key = configService.ServerKey;
        }

        public byte[] Encrypt(byte[] data)
        {
            using (var aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                aes.Key = this.Key;
                aes.GenerateIV();

                using (var transform = aes.CreateEncryptor())
                {

                    var enc = transform.TransformFinalBlock(data, 0, data.Length);
                    var checksum = ComputeHmac(enc);

                    var buf = new byte[aes.IV.Length + checksum.Length + enc.Length];

                    Buffer.BlockCopy(aes.IV, 0, buf, 0, aes.IV.Length);
                    Buffer.BlockCopy(checksum, 0, buf, aes.IV.Length, checksum.Length);
                    Buffer.BlockCopy(enc, 0, buf, aes.IV.Length + checksum.Length, enc.Length);

                    return buf;
                }
            }
        }

        public byte[] Decrypt(byte[] data)
        {
            var iv = new byte[16];
            Buffer.BlockCopy(data, 0, iv, 0, iv.Length);

            var checksum = new byte[32];
            Buffer.BlockCopy(data, 16, checksum, 0, checksum.Length);

            var enc = new byte[data.Length - 48];
            Buffer.BlockCopy(data, 48, enc, 0, data.Length - 48);

            if (!ComputeHmac(enc).SequenceEqual(checksum))
                throw new Exception("Invalid Checksum");

            using (var aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                aes.Key = this.Key;
                aes.IV = iv;

                using (var transform = aes.CreateDecryptor())
                {
                    var dec = transform.TransformFinalBlock(enc, 0, enc.Length);
                    return dec;
                }
            }
        }

        private byte[] ComputeHmac(byte[] data)
        {
            using (var hmac = new HMACSHA256(this.Key))
            {
                return hmac.ComputeHash(data);
            }
        }
    }
}
