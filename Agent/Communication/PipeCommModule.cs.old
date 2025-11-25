using Agent.Communication;
using Agent.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Agent.Helpers;
using System.Text;

namespace Agent.Models
{
    public class PipeCommModule : Communicator
    {
        public PipeCommModule(ConnexionUrl conn) : base(conn)
        {
        }


        protected override async Task<List<MessageTask>> CheckIn(List<MessageResult> results)
        {
            var client = new NamedPipeClientStream(Connexion.Address, Connexion.PipeName, PipeAccessRights.FullControl, PipeOptions.Asynchronous, System.Security.Principal.TokenImpersonationLevel.Anonymous, HandleInheritability.None);
            client.Connect(10000);

            client.SendMessage(this.Encryptor.Encrypt(results.Serialize()));

            var responseContent = client.ReceivedMessage(true);
            var dec = this.Encryptor.Decrypt(responseContent);
            var tasks = dec.Deserialize<List<MessageTask>>();

            var writer = new StreamWriter(client);
            writer.WriteLine(); //Ack to end of transfert
            writer.Flush();

            return tasks;
        }




    }
}
