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
        public const int BufferSize = 1024;
        public byte[] Buffer { get; set; }
        public string Id { get; private set; }
        public Agent Agent { get; private set; }

        public ReversePortForwardDestination Destination { get; private set; }
        public Socket Socket { get; private set; }
        public MemoryStream Stream { get; set; }

        public ReversePortForwardClient(Socket client, Agent agent, ReversePortForwardDestination dest)
        {
            this.Id = ShortGuid.NewGuid();
            this.Socket = client;
            Agent=agent;
            Destination = dest;

            Buffer = new byte[BufferSize];
            Stream = new MemoryStream();
        }

        public void WriteDataToStream(int size)
        {
            Stream.Write(Buffer, 0, size);
            Buffer = new byte[BufferSize];
        }

        public byte[] GetStreamData()
        {
            var data = Stream.ToArray();

            Stream.Dispose();
            Stream = new MemoryStream();

            return data;
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


        //public async Task<byte[]> ReadStream()
        //{
        //    var stream = this._tcp.GetStream();
        //    const int bufSize = 1024;
        //    int read;

        //    using (var ms = new MemoryStream())
        //    {
        //        do
        //        {
        //            var buf = new byte[bufSize];
        //            read = await stream.ReadAsync(buf, 0, bufSize);

        //            if (read == 0)
        //                break;

        //            await ms.WriteAsync(buf, 0, read);

        //        } while (read >= bufSize);

        //        return ms.ToArray();
        //    }
        //}
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

        private readonly Dictionary<string, ReversePortForwardClient> _clients = new Dictionary<string, ReversePortForwardClient>();
        private readonly Dictionary<int, ReversePortForwardServer> _servers = new Dictionary<int, ReversePortForwardServer>();

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
                        if (!_clients.ContainsKey(packet.Id))
                            return;

                        var client = _clients[packet.Id];
                        client.Send(packet.Data);
                    }
                    break;
                case ReversePortForwardPacket.PacketType.DISCONNECT:
                    {
                        if (!_clients.ContainsKey(packet.Id))
                            return;

                        var client = _clients[packet.Id];
                        client.Dispose();
                        this._clients.Remove(packet.Id);
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


            _servers.Add(port, server);

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
                this._clients.Add(client.Id, client);

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
                    // write received into stream
                    client.WriteDataToStream(received);

                    if (received < ReversePortForwardClient.BufferSize)
                    {
                        // send data to TS
                        var packet = new ReversePortForwardPacket(client.Id, ReversePortForwardPacket.PacketType.DATA, client.GetStreamData());
                        var f = this._frameService.CreateFrame(client.Agent.MetaData.Id, NetFrameType.ReversePortForward, packet);
                        await client.Agent.SendFrame(f);
                    }
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
            catch (ObjectDisposedException ex)
            {
#if DEBUG
                Debug.WriteLine($"RPORTForward Error : {ex}");
#endif
                var packet = new ReversePortForwardPacket() { Id = client.Id, Type = ReversePortForwardPacket.PacketType.DISCONNECT };
                var f = this._frameService.CreateFrame(client.Agent.MetaData.Id, NetFrameType.ReversePortForward, packet);
                await client.Agent.SendFrame(f);
            }
        }

        public async Task<bool> StopServer(int port)
        {
            if (!this._servers.ContainsKey(port))
                return false;

            var server = this._servers[port];
            this._servers.Remove(port);


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