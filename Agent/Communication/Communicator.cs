using Agent.Models;
using Agent.Service;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Communication
{

   
    public abstract class Communicator
    {

        public CommunicationType CommunicationType { get; protected set; }

        public abstract event Func<NetFrame, Task> FrameReceived;
        public abstract event Action OnException;

        public bool IsRunning { get; protected set; } = false;
 
        public Agent Agent { get; protected set; }

        private Random random = new Random();

        public string ServerKey { get; private set; }

        public ConnexionUrl Connexion { get; set; }

        //public IProxyService ProxyService { get; protected set; }
        public INetworkService NetworkeService { get; protected set; }
        public IFileService FileService { get; protected set; }

        public IConfigService ConfigService { get; protected set; }

        public Communicator(ConnexionUrl connection)
        {
            this.Connexion = connection;
            this.ConfigService = ServiceProvider.GetService<IConfigService>();
            this.NetworkeService = ServiceProvider.GetService<INetworkService>();
            this.FileService = ServiceProvider.GetService<IFileService>();
            //this.ProxyService =ServiceProvider.GetService<IProxyService>();
        }

        public virtual void Init(Agent agent)
        {
            this.Agent = agent;
            this._tokenSource = agent.TokenSource;
        }

        public abstract Task Start();

        public abstract Task Run();

        public virtual async Task Stop()
        {
            if (!this.IsRunning)
                return;

            this._tokenSource.Cancel();
            this.IsRunning = false;
        }

        protected CancellationTokenSource _tokenSource;

        public abstract Task SendFrame(NetFrame frame);

    }
}
