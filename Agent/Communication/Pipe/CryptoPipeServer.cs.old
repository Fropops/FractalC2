
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Models
{
    public class CryptoPipeServer : PipeServer
    {
        public CryptoPipeServer(string pipeName, PipeCommModule module) : base(pipeName, module)
        {
        }

        // Creates a PipeSecurity that allows users read/write access


        protected override void RunServer()
        {
            //using (NamedPipeServerStream server = new NamedPipeServerStream(this.PipeName, PipeDirection.InOut, 10, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
            //{
            //    server.SetAccessControl(CreatePipeSecurityForEveryone());

            using (NamedPipeServerStream server = new NamedPipeServerStream(this.PipeName, PipeDirection.InOut, 10, PipeTransmissionMode.Byte, PipeOptions.Asynchronous,512,512,CreatePipeSecurityForEveryone(), HandleInheritability.None))
            {
                Debug.WriteLine("CrytpoPipeServer : Server started waiting for incomming connections.");
                while (!this._cancel.IsCancellationRequested)
                {
                    try
                    {
                        if (!server.WaitForConnection(this._cancel))
                            return;

                        //Console.WriteLine("[thread: {0}] -> Client connected.", Thread.CurrentThread.ManagedThreadId);


                        // Create a new RSA crypto provider
                        RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                        rsa.KeySize = 2048; // Set the key size to 2048 bits
                        rsa.ExportParameters(true); // Export the private key

                        //Console.WriteLine("Public key: {0}", rsa.ToXmlString(false));
                        //Console.WriteLine("Private key: {0}", rsa.ToXmlString(true));



                        var reader = new StreamReader(server);
                        var writer = new StreamWriter(server);

                        //Sends the public key
                        writer.WriteLine(rsa.ToXmlString(false));
                        writer.Flush();

                        //Read the encoded generated symetric key
                        var b64EncryptedKey = reader.ReadLine();
                        var receivedKey = Convert.FromBase64String(b64EncryptedKey);
                        byte[] decryptedKey = rsa.Decrypt(receivedKey, false);

                        //Read the encoded generated symetric IV
                        var b64EncryptedIv = reader.ReadLine();
                        var receivedIv = Convert.FromBase64String(b64EncryptedIv);
                        byte[] decryptedIv = rsa.Decrypt(receivedIv, false);

                        // Console.WriteLine($"B64 Key = {Convert.ToBase64String(decryptedKey)}");

                        //Console.WriteLine("KEY = " + string.Join(",", decryptedKey.Select(i => i.ToString())));
                        //Console.WriteLine("IV = " + string.Join(",", decryptedIv.Select(i => i.ToString())));

                        RijndaelManaged rijndael = new RijndaelManaged();
                        rijndael.KeySize = 256;
                        rijndael.BlockSize = 128;
                        rijndael.Key = decryptedKey;
                        rijndael.IV = decryptedIv;
                        //rijndael.Padding = PaddingMode.Zeros;

                        //Console.WriteLine("Server - Key Exchange done !");

                        var b64tasks = this.ReceiveMessageAndDecrypt(server, rijndael);
                        var tasks = Convert.FromBase64String(b64tasks).Deserialize<List<MessageTask>>();
                        this.PipeCommModule.MessageService.EnqueueTasks(tasks);
                        //Console.WriteLine("Pipe Client Tasks received", Thread.CurrentThread.ManagedThreadId);

                        var agentId = this.PipeCommModule.MessageService.AgentMetaData.Id;

                        var results = this.PipeCommModule.MessageService.GetMessageResultsToRelay();

                        if (!results.Any(t => t.Header.Owner == agentId))
                        {
                            //add a checkin message
                            var messageResult = new MessageResult();
                            messageResult.Header.Owner = agentId;
                            messageResult.FileChunk = this.PipeCommModule.FileService.GetChunkToSend();
                            messageResult.ProxyMessages = this.PipeCommModule.ProxyService.GetResponses();
                            results.Add(messageResult);
                        }
                        else
                        {
                            foreach (var mess in results)
                                mess.FileChunk = this.PipeCommModule.FileService.GetChunkToSend();
                            results.First().ProxyMessages = this.PipeCommModule.ProxyService.GetResponses();
                        }


                        foreach (var resMess in results)
                            resMess.Header.Path.Insert(0, agentId);


                        string b64results = Convert.ToBase64String(results.Serialize());
                        this.EncryptAndSendMessage(server, rijndael, b64results);
                        //Console.WriteLine("Pipe Client result sent", Thread.CurrentThread.ManagedThreadId);

                        //Get all relays
                        var allrelays = this.PipeCommModule.Links.SelectMany(l => l.Relays).ToList();
                        allrelays.Add(this.PipeCommModule.MessageService.AgentMetaData.Id);
                        string b64Relays = Convert.ToBase64String(allrelays.Serialize());
                        this.EncryptAndSendMessage(server, rijndael, b64Relays);
                        //Console.WriteLine("Pipe Client Relay sent.", Thread.CurrentThread.ManagedThreadId);

                        if (server.IsConnected)
                        {
                            //Console.WriteLine("Pipe Client Disconnected.", Thread.CurrentThread.ManagedThreadId);
                            server.Disconnect();
                        }

                        //Console.WriteLine("Pipe Client Leaved.", Thread.CurrentThread.ManagedThreadId);
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        Console.WriteLine(ex.ToString());
#endif
                    }
                }
            }
        }


        private void EncryptAndSendMessage(NamedPipeServerStream server, RijndaelManaged rijndael, string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            //Console.WriteLine($"messageBytes = " + string.Join(",", messageBytes.Select(i => i.ToString())));
            var encryptedMessageBytes = rijndael.CreateEncryptor().TransformFinalBlock(messageBytes, 0, messageBytes.Length);
            var b64EncryptedMessage = Convert.ToBase64String(encryptedMessageBytes);
            var writer = new StreamWriter(server);
            //Console.WriteLine($"B64 Sent Message = {b64EncryptedMessage}");
            writer.WriteLine(b64EncryptedMessage);
            writer.Flush();
        }

        private string ReceiveMessageAndDecrypt(NamedPipeServerStream server, RijndaelManaged rijndael)
        {
            var reader = new StreamReader(server);
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
