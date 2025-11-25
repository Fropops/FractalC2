using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Models
{
    public abstract class PipeClient
    {
        public string Hostname { get; protected set; }

        public string PipeName { get; protected set; }
        public PipeClient(string hostname, string pipename)
        {
            this.Hostname = hostname;
            this.PipeName = pipename;
        }

        public abstract Tuple<List<MessageResult>, List<string>> SendAndReceive(List<MessageTask> tasks);
    }
}
