using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BinarySerializer;
using Shared;
using TeamServer.Forwarding;

namespace TeamServer.Services
{
    public interface IReversePortForwardService
    {
        Task<bool> StartClient(string id, string agentId, ReversePortForwardDestination destination);
        Task<bool> StopClient(string id);

        bool Contains(string agentId);

        RPortFwdClient GetClientById(string id);
    }
    public class ReversePortForwardService : IReversePortForwardService
    {
        private bool _log = false;

        private readonly IAgentService _agentService;
        private readonly IFrameService _frameService;
        public ReversePortForwardService(IAgentService agentService, IFrameService frameService)
        {
            this._agentService = agentService;
            this._frameService = frameService;
        }
        private Dictionary<string, RPortFwdClient> _RPortFwrdClients { get; set; } = new Dictionary<string, RPortFwdClient>();

        public bool Contains(string id)
        {
            return this._RPortFwrdClients.ContainsKey(id);
        }

        public RPortFwdClient GetClientById(string id)
        {
            if (!_RPortFwrdClients.ContainsKey(id))
                return null;

            return _RPortFwrdClients[id];
        }


        public async Task<bool> StartClient(string id, string agentId, ReversePortForwardDestination destination)
        {
            var client = new RPortFwdClient(id, agentId);

            if (!client.Connect(destination))
            {
                if (this._log)
                    Logger.Log($"PFWD [{client.Id}] : connection refused.");
                var packet = new ReversePortForwardPacket(client.Id, ReversePortForwardPacket.PacketType.DISCONNECT);
                _frameService.CacheFrame(client.AgentId, NetFrameType.ReversePortForward, packet);
                return false;
            }

            this._RPortFwrdClients.Add(id, client);

            if (this._log)
                Logger.Log($"PFWD [{client.Id}] : connected.");

            // handle client in new thread
            var thread = new Thread(HandleClient);
            thread.Start(client);

            return true;
        }

        public async Task<bool> StopClient(string id)
        {
            if (!this._RPortFwrdClients.ContainsKey(id))
                return false;

            var client = this._RPortFwrdClients[id];
            this._RPortFwrdClients.Remove(id);

            if (this._log)
                Logger.Log($"PFWD [{client.Id}] : disconnect.");

            client.Dispose();

            return true;
        }

        private async void HandleClient(object obj)
        {
            if (obj is not RPortFwdClient client)
                return;

            if (this._log)
                Logger.Log($"PFWD [{client.Id}] : Connecting...");

            ReversePortForwardPacket packet = null;

            try
            {
                // drop into a loop
                while (true)
                {
                    // if client has data
                    if (client.DataAvailable())
                    {
                        // read it
                        var data = await client.ReadStream();

                        if (this._log)
                            Logger.Log($"PFWD [{client.Id}] : Data [{data.Length}].");

                        // send to the drone
                        packet = new ReversePortForwardPacket(client.Id, ReversePortForwardPacket.PacketType.DATA, data);
                        _frameService.CacheFrame(client.AgentId, NetFrameType.ReversePortForward, packet);
                    }

                    byte[] response;
                    if (client.TryDequeue(out response))
                    {
                        await client.WriteStream(response);

                        if (this._log)
                            Logger.Log($"PFWD [{client.Id}] : Response [{response.Length}].");

                    }

                    await Task.Delay(100);
                }
            }
            catch (Exception e)
            {
                if (_log)
                    Logger.Log($"PFWD [{client.Id}] : Exception {e}.");
            }

            // send a disconnect
            packet = new ReversePortForwardPacket(client.Id, ReversePortForwardPacket.PacketType.DISCONNECT);
            _frameService.CacheFrame(client.AgentId, NetFrameType.Socks, packet);
            if (this._log)
                Logger.Log($"PFWD [{client.Id}] : Disconnect.");
            this._RPortFwrdClients.Remove(client.Id);
            client.Dispose();
        }
    }
}
