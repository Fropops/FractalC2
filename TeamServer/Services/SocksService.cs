using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TeamServer.Forwarding;

namespace TeamServer.Services
{
    [InjectableService]
    public interface ISocksService
    {
        Task<bool> StartProxy(string agentId, int port);
        Task<bool> StopProxy(int port);

        bool Contains(string agentId);
        bool Contains(int portId);

        SocksClient GetClientById(string agentId, string socksProxyId);
        List<KeyValuePair<int, SocksProxy>> GetProxies();
    }

    [InjectableServiceImplementation(typeof(ISocksService))]
    public class SocksService : ISocksService
    {
        private readonly IAgentService _agentService;
        private readonly IFrameService _frameService;
        public SocksService(IAgentService agentService, IFrameService frameService)
        {
            this._agentService = agentService;
            this._frameService = frameService;
        }
        private Dictionary<int, SocksProxy> ProxiesByPort { get; set; } = new Dictionary<int, SocksProxy>();
        private Dictionary<string, SocksProxy> ProxiesByAgent { get; set; } = new Dictionary<string, SocksProxy>();

        public bool Contains(int portId)
        {
            return this.ProxiesByPort.ContainsKey(portId);
        }

        public bool Contains(string agentId)
        {
            return this.ProxiesByAgent.ContainsKey(agentId);
        }

        public List<KeyValuePair<int, SocksProxy>> GetProxies()
        {
            return ProxiesByPort.ToList();
        }

        public SocksClient GetClientById(string agentId, string socksProxyId)
        {
            if (!ProxiesByAgent.ContainsKey(agentId))
                return null;

            var proxy = ProxiesByAgent[agentId];
            return proxy.GetSocksClient(socksProxyId);
        }

        public async Task<bool> StartProxy(string agentId, int port)
        {
            if (this.ProxiesByPort.ContainsKey(port) || this.ProxiesByAgent.ContainsKey(agentId))
                return false;

            var agent = this._agentService.GetAgent(agentId);

            var proxy = new SocksProxy(agent.Id, port, this._frameService);
            
            _ = proxy.Start();

            await Task.Delay(1000);

            if (proxy.IsRunning)
            {
                this.ProxiesByAgent.Add(agentId, proxy);
                this.ProxiesByPort.Add(port, proxy);
            }

            return proxy.IsRunning;
        }
        public async Task<bool> StopProxy(int portId)
        {
            if(!this.ProxiesByPort.ContainsKey(portId))
                return false;

            

            var proxy = this.ProxiesByPort[portId];
            await proxy.Stop();

            var agentId = proxy.AgentId;
            this.ProxiesByAgent.Remove(agentId);
            this.ProxiesByPort.Remove(portId);
            return true;
        }
    }
}
