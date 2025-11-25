using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TeamServer.Forwarding;

namespace TeamServer.Services
{
    public interface ISocksService
    {
        Task<bool> StartProxy(string agentId, int port);
        Task<bool> StopProxy(string agentId);

        bool Contains(string agentId);

        SocksClient GetClientById(string agentId, string socksProxyId);
        List<KeyValuePair<string, SocksProxy>> GetProxies();
    }
    public class SocksService : ISocksService
    {
        private readonly IAgentService _agentService;
        private readonly IFrameService _frameService;
        public SocksService(IAgentService agentService, IFrameService frameService)
        {
            this._agentService = agentService;
            this._frameService = frameService;
        }
        private Dictionary<string, SocksProxy> Proxies { get; set; } = new Dictionary<string, SocksProxy>();

        public bool Contains(string agentId)
        {
            return this.Proxies.ContainsKey(agentId);
        }

        public List<KeyValuePair<string, SocksProxy>> GetProxies()
        {
            return Proxies.ToList();
        }

        public SocksClient GetClientById(string agentId, string socksProxyId)
        {
            if (!Proxies.ContainsKey(agentId))
                return null;

            var proxy = Proxies[agentId];
            return proxy.GetSocksClient(socksProxyId);
        }

        public async Task<bool> StartProxy(string agentId, int port)
        {
            if (this.Proxies.ContainsKey(agentId))
                return false;

            var agent = this._agentService.GetAgent(agentId);

            var proxy = new SocksProxy(agent.Id, port, this._frameService);
            
            _ = proxy.Start();

            await Task.Delay(1000);
            
            if(proxy.IsRunning)
                this.Proxies.Add(agentId, proxy);

            return proxy.IsRunning;
        }
        public async Task<bool> StopProxy(string agentId)
        {
            if(!this.Proxies.ContainsKey(agentId))
                return false;

            var proxy = this.Proxies[agentId];
            await proxy.Stop();

            this.Proxies.Remove(agentId);
            return true;
        }
    }
}
