using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agent.Communication;
using Agent.Helpers;
using Agent.Models;

namespace Agent.Service.Pivoting
{
    public class PivotTCPServer : PivotServer
    {

        public PivotTCPServer(ConnexionUrl conn, string serverKey) : base(conn, serverKey)
        {
        }


        public override async Task Start()
        {
            try
            {
                this.Status = RunningService.RunningStatus.Running;

                var listener = new TcpListener(IPAddress.Parse(Connexion.Address), this.Connexion.Port);
                listener.Start(100);

                while (!_tokenSource.IsCancellationRequested)
                {
                    // this blocks until a connection is received or token is cancelled
                    var client = await listener.AcceptTcpClientAsync(_tokenSource);

                    // do something with the connected client
                    var thread = new Thread(async () => await HandleClient(client));
                    thread.Start();
                }
                // handle client in new thread

                listener.Stop();
            }
            finally
            {
                this.Status = RunningService.RunningStatus.Stoped;
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            if (client == null)
                return;

            var endpoint = (IPEndPoint)client.Client.RemoteEndPoint;
            var id = endpoint.Address + ":" + endpoint.Port;

            try
            {
                while (!_tokenSource.IsCancellationRequested && client.IsAlive())
                {

                    // read from client
                    if (!client.DataAvailable())
                        continue;

                    this.Handle(client);

                    // sos cpu
                    await Task.Delay(10);
                }

            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex);
#endif
            }
            finally
            {
                Debug.WriteLine($"TCP Pivot {id} : disconnected");
            }
        }

        private void Handle(TcpClient client)
        {
            var req = client.ReceivedData();
            var dec= this.Encryptor.Decrypt(req);

            var responses = dec.Deserialize<List<MessageResult>>();
            _messageService.EnqueueResults(responses);

            var relays = this.ExtractRelays(responses);

            Debug.WriteLine($"TCP Pivot {Connexion.ToString()} Sending task to Relays {string.Join(",", relays)}");
            var tasks = this._messageService.GetMessageTasksToRelay(relays);
            var ser = tasks.Serialize();
            var enc = this.Encryptor.Encrypt(ser);
            client.SendMessage(enc);
        }
    }
}
