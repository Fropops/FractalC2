using Agent.Models;
using Agent.Service;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Agent.Helpers;
using System.Net;
using Shared;
using BinarySerializer;
using System.Threading;
using System.IO;

namespace Agent.Service
{
    public sealed class ReversePortForwardServer : IDisposable
    {

        public int Port { get; private set; }
        public TcpListener Listener { get; private set; }
        public Agent Agent { get; private set; }

        public ReversePortForwardDestination Destination { get; private set; }


        public ReversePortForwardServer(int port, TcpListener listener, Agent agent, ReversePortForwardDestination dest)
        {
            Port=port;
            Listener=listener;
            Agent =agent;
            Destination = dest;
        }

        public void Dispose()
        {
            Listener.Stop();
        }
    }

    public sealed class ReversePortForwardClient : IDisposable
    {
        public const int BufferSize = 32768;
        public byte[] Buffer { get; set; }
        public string Id { get; private set; }
        public Agent Agent { get; private set; }

        public ReversePortForwardDestination Destination { get; private set; }
        public Socket Socket { get; private set; }

        public ReversePortForwardClient(Socket client, Agent agent, ReversePortForwardDestination dest)
        {
            this.Id = ShortGuid.NewGuid();
            this.Socket = client;
            Agent=agent;
            Destination = dest;

            Buffer = new byte[BufferSize];
        }

        public void Send(byte[] data)
        {
            Socket.Send(data);
        }

        public void Disconnect()
        {
            try
            {
                if (this.Socket == null)
                    return;
                this.Socket.Disconnect(false);
            }
            finally { }
        }


        public bool IsConnected()
        {
            return this.Socket.Connected;
        }

        public void Dispose()
        {
            this.Disconnect();
        }
    }
    internal interface IReversePortForwardService
    {
        Task HandlePacket(ReversePortForwardPacket packet, Agent agent);

        Task<bool> StartServer(int port, Agent agent, ReversePortForwardDestination dest);
        Task<bool> StopServer(int port);
        List<ReversePortForwardServer> GetServers();
    }
    internal class ReversePortForwardService : IReversePortForwardService
    {
        private readonly IFrameService _frameService;
        public ReversePortForwardService(IFrameService frameService)
        {
            _frameService = frameService;
        }

        private readonly ConcurrentDictionary<string, ReversePortForwardClient> _clients = new ConcurrentDictionary<string, ReversePortForwardClient>();
        private readonly ConcurrentDictionary<int, ReversePortForwardServer> _servers = new ConcurrentDictionary<int, ReversePortForwardServer>();

        public List<ReversePortForwardServer> GetServers()
        {
            return _servers.Values.ToList();
        }

        public async Task HandlePacket(ReversePortForwardPacket packet, Agent agent)
        {
            switch (packet.Type)
            {
                case ReversePortForwardPacket.PacketType.DATA:
                    {
                        if (!_clients.TryGetValue(packet.Id, out var client))
                            return;

                        client.Send(packet.Data);
                    }
                    break;
                case ReversePortForwardPacket.PacketType.DISCONNECT:
                    {
                        if (!_clients.TryGetValue(packet.Id, out var client))
                            return;

                        client.Dispose();
                        this._clients.TryRemove(packet.Id, out _);
                    }
                    break;
                default: break;
            }


        }


        public async Task<bool> StartServer(int port, Agent agent, ReversePortForwardDestination dest)
        {
            if (this._servers.ContainsKey(port))
                return false;

            var listener = new TcpListener(new IPEndPoint(IPAddress.Any, port));

            try
            {
                listener.Start(100);
            }
            catch (Exception ex)
            {
                return false;
            }

            var server = new ReversePortForwardServer(port, listener, agent, dest);
            listener.BeginAcceptSocket(ClientAcceptedCallback, server);


            _servers[port] = server;

            return true;
        }

        private async void ClientAcceptedCallback(IAsyncResult ar)
        {
            ReversePortForwardServer server = ar.AsyncState as ReversePortForwardServer;
            if (server == null)
                return;

            try
            {
                var socket = server.Listener.EndAcceptSocket(ar);

                //restart listener
                server.Listener.BeginAcceptSocket(ClientAcceptedCallback, server);

                var client = new ReversePortForwardClient(socket, server.Agent, server.Destination);
                this._clients[client.Id] = client;

                //Connect
                var packet = new ReversePortForwardPacket(client.Id, ReversePortForwardPacket.PacketType.CONNECT, await server.Destination.BinarySerializeAsync());
                var f = this._frameService.CreateFrame(client.Agent.MetaData.Id, NetFrameType.ReversePortForward, packet);
                await client.Agent.SendFrame(f);

#if DEBUG
                Debug.WriteLine($"RPORTForward Client connected : {client.Id}");
#endif

                // receive from socket
                socket.BeginReceive(
                    client.Buffer,
                    0,
                    ReversePortForwardClient.BufferSize,
                    SocketFlags.None,
                    ClientReceiveCallback,
                    client);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine($"RPORTForward Error : {ex}");
#endif
            }
        }

        private async void ClientReceiveCallback(IAsyncResult ar)
        {
            ReversePortForwardClient client = ar.AsyncState as ReversePortForwardClient;
            if (client == null)
                return;

            try
            {
                var received = client.Socket.EndReceive(ar);
                if (received != 0)
                {
#if DEBUG
                    Debug.WriteLine($"RPORTForward Client Data : {client.Id}");
#endif
                    var data = new byte[received];
                    Buffer.BlockCopy(client.Buffer, 0, data, 0, received);

                    // send data to TS immediately
                    var packet = new ReversePortForwardPacket(client.Id, ReversePortForwardPacket.PacketType.DATA, data);
                    var f = this._frameService.CreateFrame(client.Agent.MetaData.Id, NetFrameType.ReversePortForward, packet);
                    await client.Agent.SendFrame(f);
                }
                else
                {
                    // client disconnected
                    client.Dispose();
                    this._clients.TryRemove(client.Id, out _);

                    var packet = new ReversePortForwardPacket() { Id = client.Id, Type = ReversePortForwardPacket.PacketType.DISCONNECT };
                    var f = this._frameService.CreateFrame(client.Agent.MetaData.Id, NetFrameType.ReversePortForward, packet);
                    await client.Agent.SendFrame(f);
                    return;
                }

                //loop until exception
                client.Socket.BeginReceive(
                    client.Buffer,
                    0,
                    ReversePortForwardClient.BufferSize,
                    SocketFlags.None,
                    ClientReceiveCallback,
                    client);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine($"RPORTForward Error : {ex}");
#endif
                try
                {
                    var packet = new ReversePortForwardPacket() { Id = client.Id, Type = ReversePortForwardPacket.PacketType.DISCONNECT };
                    var f = this._frameService.CreateFrame(client.Agent.MetaData.Id, NetFrameType.ReversePortForward, packet);
                    await client.Agent.SendFrame(f);
                }
                catch { }
            }
        }

        public async Task<bool> StopServer(int port)
        {
            if (!this._servers.TryRemove(port, out var server))
                return false;


            try
            {
                server.Dispose();
            }
            catch { }

            foreach (var client in this._clients.Values.Where(c => c.Destination == server.Destination))
            {
                try
                {
                    client.Dispose();
                }
                catch { }

                var packet = new ReversePortForwardPacket() { Id = client.Id, Type = ReversePortForwardPacket.PacketType.DISCONNECT };
                var f = this._frameService.CreateFrame(client.Agent.MetaData.Id, NetFrameType.ReversePortForward, packet);
                await client.Agent.SendFrame(f);
            }

            return true;
        }
    }
}