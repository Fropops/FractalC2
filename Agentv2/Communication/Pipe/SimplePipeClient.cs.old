using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Models
{
    public class SimplePipeClient : PipeClient
    {
        public SimplePipeClient(string hostname, string pipename) : base(hostname, pipename)
        {
                
        }

        public override Tuple<List<MessageResult>, List<string>> SendAndReceive(List<MessageTask> tasks)
        {
            var pipeClient = new NamedPipeClientStream(this.Hostname, this.PipeName, PipeDirection.InOut);
            pipeClient.Connect();

            var writer = new StreamWriter(pipeClient);
            var reader = new StreamReader(pipeClient);

            string b64tasks = Convert.ToBase64String(tasks.Serialize());
            writer.WriteLine(b64tasks);
            writer.Flush();

            
            var b64results = reader.ReadLine();
            var b64relays = reader.ReadLine();

            var messages = Convert.FromBase64String(b64results).Deserialize<List<MessageResult>>();
            var relays = Convert.FromBase64String(b64relays).Deserialize<List<string>>();

            // Close the client
            pipeClient.Close();

            return new Tuple<List<MessageResult>, List<string>>(messages, relays);
        }
    }
}
