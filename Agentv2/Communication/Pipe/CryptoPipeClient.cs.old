using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Models
{
    public class CryptoPipeClient : PipeClient
    {
        public CryptoPipeClient(string hostname, string pipename) : base(hostname, pipename)
        {
                
        }

        public override Tuple<List<MessageResult>, List<string>> SendAndReceive(List<MessageTask> tasks)
        {
            //Console.WriteLine("Named Pipe Client");

            // Connect to the server using a unique pipe name
            var pipeClient = new NamedPipeClientStream(this.Hostname, this.PipeName, PipeAccessRights.FullControl, PipeOptions.Asynchronous, System.Security.Principal.TokenImpersonationLevel.Anonymous, HandleInheritability.None);
            pipeClient.Connect(10000);

            // Send a message to the server
            var writer = new StreamWriter(pipeClient);
            var reader = new StreamReader(pipeClient);

            //Console.WriteLine("waiting public key");
            string xmlPubKey = reader.ReadLine();

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(xmlPubKey);

            //Generate symmetric Key
            RijndaelManaged rijndael = new RijndaelManaged();
            rijndael.KeySize = 256; // Set the key size to 256 bits
            rijndael.BlockSize = 128;
            rijndael.GenerateKey(); // Generate a random key
            //rijndael.Padding = PaddingMode.Zeros;

            //Console.WriteLine("KEY = " + string.Join(",", rijndael.Key.Select(i => i.ToString())));
            //Console.WriteLine("IV = " + string.Join(",", rijndael.IV.Select(i => i.ToString())));
            //Console.WriteLine(" = " + string.Join(",", rijndael..Select(i => i.ToString())));

            //Console.WriteLine($"B64 Key = {Convert.ToBase64String(rijndael.Key)}");

            // Encrypt the symmetric key using the public key
            byte[] encryptedKey = rsa.Encrypt(rijndael.Key, false);
            var b64EncryptedKey = Convert.ToBase64String(encryptedKey);

            byte[] encryptedIV = rsa.Encrypt(rijndael.IV, false);
            var b64EncryptedIV = Convert.ToBase64String(encryptedIV);

            //Console.WriteLine("Sending symetric key");
            writer.WriteLine(b64EncryptedKey);
            writer.Flush();

            //Console.WriteLine("Sending symetric IV");
            writer.WriteLine(b64EncryptedIV);
            writer.Flush();

            //Console.WriteLine("Client - Key Exchange done !");

            string b64tasks = Convert.ToBase64String(tasks.Serialize());
            this.EncryptAndSendMessage(pipeClient, rijndael, b64tasks);

            
            var b64results = this.ReceiveMessageAndDecrypt(pipeClient, rijndael);
            var b64relays = this.ReceiveMessageAndDecrypt(pipeClient, rijndael);

            var messages = Convert.FromBase64String(b64results).Deserialize<List<MessageResult>>();
            var relays = Convert.FromBase64String(b64relays).Deserialize<List<string>>();

            // Close the client
            pipeClient.Close();

            return new Tuple<List<MessageResult>, List<string>>(messages, relays);
        }

        private void EncryptAndSendMessage(NamedPipeClientStream stream, RijndaelManaged rijndael, string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            //Console.WriteLine("messageBytes = " + string.Join(",", messageBytes.Select(i => i.ToString())));
            var encryptedMessageBytes = rijndael.CreateEncryptor().TransformFinalBlock(messageBytes, 0, messageBytes.Length);
            var b64EncryptedMessage = Convert.ToBase64String(encryptedMessageBytes);
            //Console.WriteLine($"B64 Sent Message = {b64EncryptedMessage}");

            var writer = new StreamWriter(stream);
            writer.WriteLine(b64EncryptedMessage);
            writer.Flush();
        }

        private string ReceiveMessageAndDecrypt(NamedPipeClientStream stream, RijndaelManaged rijndael)
        {
            var reader = new StreamReader(stream);
            var b64EncryptedMessage = reader.ReadLine();
            //Console.WriteLine($"B64 Received Message = {b64EncryptedMessage}");
            var encryptedMessageBytes = Convert.FromBase64String(b64EncryptedMessage);
            byte[] decryptedBytes = rijndael.CreateDecryptor().TransformFinalBlock(encryptedMessageBytes, 0, encryptedMessageBytes.Length);
            //Console.WriteLine($"decryptedBytes = " + string.Join(",", decryptedBytes.Select(i => i.ToString())));
            string decrypted = Encoding.UTF8.GetString(decryptedBytes);

            return decrypted;
        }
    }
}
