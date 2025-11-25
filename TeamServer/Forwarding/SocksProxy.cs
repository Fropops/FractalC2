using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BinarySerializer;
using Common;
using Shared;
using TeamServer.Helper;
using TeamServer.Services;

namespace TeamServer.Forwarding
{
    public sealed class SocksClient : IDisposable
    {
        public string Id { get; private set; }
        private TcpClient _tcp;
        private readonly ManualResetEvent _signal = new ManualResetEvent(false);
        private ConcurrentQueue<byte[]> _dataQueue = new ConcurrentQueue<byte[]>();
        public bool? ConnexionResult { get; private set; } = null;

        public SocksClient(TcpClient client)
        {
            this.Id = ShortGuid.NewGuid();
            this._tcp = client;
        }

        public void QueueData(byte[] data)
        {
            _dataQueue.Enqueue(data);
        }

        public bool TryDequeue(out byte[] data)
        {
            return _dataQueue.TryDequeue(out data);
        }

        public void Unblock(bool connectionSucceed)
        {
            ConnexionResult = connectionSucceed;
            _signal.Set();
        }

        public void WaitConnectionResult()
        {
            _signal.WaitOne();
        }

        public void Disconnect()
        {
            try
            {
                this._tcp.Close();
            }
            finally { }
        }

        public bool DataAvailable()
        {
            return this._tcp.DataAvailable();
        }

        public bool IsConnected()
        {
            return this._tcp.IsAlive();
        }

        public async Task<byte[]> ReadStream()
        {
            var stream = this._tcp.GetStream();
            const int bufSize = 1024;
            int read;

            using (var ms = new MemoryStream())
            {
                do
                {
                    var buf = new byte[bufSize];
                    read = await stream.ReadAsync(buf, 0, bufSize);

                    if (read == 0)
                        break;

                    await ms.WriteAsync(buf, 0, read);

                } while (read >= bufSize);

                return ms.ToArray();
            }
        }

        public async Task WriteStream(byte[] data)
        {
            var stream = this._tcp.GetStream();
            await stream.WriteAsync(data, 0, data.Length);
        }

        public async Task<Socks4ConnectRequest> ReadConnectRequest()
        {
            var stream = this._tcp.GetStream();
            var data = await stream.ReadStream();
            if (data.Length < 9)
                return null;
            return new Socks4ConnectRequest(data) { Id = this.Id };
        }

        public async Task SendConnectReply(bool success)
        {
            var stream = this._tcp.GetStream();
            var reply = new byte[]
            {
            0x00,
            success ? (byte)0x5a : (byte)0x5b,
            0x00, 0x00,
            0x00, 0x00, 0x00, 0x00
            };

            await stream.WriteStream(reply);
        }

        public void Dispose()
        {
            if (this._tcp != null)
                this._tcp.Dispose();
        }
    }


    public sealed class SocksProxy
    {
        private bool _log = false;
        public string AgentId { get; set; }
        public int BindPort { get; set; }

        public bool IsRunning { get; private set; }

        private readonly IFrameService _frameService;

        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        private Dictionary<string, SocksClient> _socksClients = new Dictionary<string, SocksClient>();

        public SocksClient GetSocksClient(string socksProxyId)
        {
            if (!_socksClients.ContainsKey(socksProxyId))
                return null;

            return _socksClients[socksProxyId];
        }

        public SocksProxy(string agentId, int port, IFrameService frameService)
        {
            this.AgentId = agentId;
            this.BindPort = port;
            this._frameService = frameService;
        }

        public async Task Start()
        {
            this.IsRunning = true;
            var listener = new TcpListener(new IPEndPoint(IPAddress.Any, BindPort));
            try
            {
                listener.Start(100);
            }
            catch
            {
                this.IsRunning = false;
                return;
            }

            
            while (!_tokenSource.IsCancellationRequested)
            {
                // wait for client
                // this will throw an OperationCancelledException if the token is cancelled
                try
                {
                    var client = await listener.AcceptTcpClientAsync(_tokenSource.Token);

                    // handle client in new thread
                    var thread = new Thread(HandleClient);

                    var sockClient = new SocksClient(client);
                    this._socksClients.Add(sockClient.Id, sockClient);

                    thread.Start(sockClient);
                }
                catch (OperationCanceledException)
                {
                    // ignore and proceed to stop the listener
                }
            }

            listener.Stop();
            this.IsRunning = false;
        }



        private async void HandleClient(object obj)
        {
            if (obj is not SocksClient client)
                return;

            if (this._log)
                Logger.Log($"SOCKS [{client.Id}] : Connecting...");


            // first thing is to read the connect request
            var connectReq = await client.ReadConnectRequest();

            if (connectReq == null)
            {
                if (this._log)
                    Logger.Log($"SOCKS [{client.Id}] : Wrong Connect request...");
                return;
            }

            // if not version 4, send error
            if (connectReq.Version != 4)
            {
                await client.SendConnectReply(false);
                return;
            }

            // otherwise, send "connect" task to drone
            var packet = new Socks4Packet(client.Id, Socks4Packet.PacketType.CONNECT, connectReq.BinarySerializeAsync().Result);
            _frameService.CacheFrame(AgentId, Shared.NetFrameType.Socks, packet);

            // wait for confirmation from drone
            client.WaitConnectionResult();

            if (client.ConnexionResult != true)
            {
                if (this._log)
                    Logger.Log($"SOCKS [{client.Id}] : Coonnexion refused.");

                client.Dispose();
                _socksClients.Remove(client.Id);
                return;
            }

            if (this._log)
                Logger.Log($"SOCKS [{client.Id}] : Coonnexion succeed.");

            // send success back to client
            await client.SendConnectReply(true);

            try
            {
                // drop into a loop
                while (!_tokenSource.IsCancellationRequested && client.IsConnected())
                {

                    // if client has data
                    if (client.DataAvailable())
                    {
                        // read it
                        var data = await client.ReadStream();

                        if (this._log)
                            Logger.Log($"SOCKS [{client.Id}] : Data [{data.Length}].");

                        // send to the drone
                        packet = new Socks4Packet(client.Id, Socks4Packet.PacketType.DATA, data);
                        _frameService.CacheFrame(AgentId, Shared.NetFrameType.Socks, packet);
                    }

                    byte[] response;
                    if (client.TryDequeue(out response))
                    {
                        await client.WriteStream(response);

                        if (this._log)
                            Logger.Log($"SOCKS [{client.Id}] : Response [{response.Length}].");

                    }

                    await Task.Delay(100);
                }

                // send a disconnect
                packet = new Socks4Packet(client.Id, Socks4Packet.PacketType.DISCONNECT);
                _frameService.CacheFrame(AgentId, NetFrameType.Socks, packet);
                if (this._log)
                    Logger.Log($"SOCKS [{client.Id}] : Disconnect.");
                _socksClients.Remove(client.Id);
                client.Dispose();
            }
            catch (Exception e)
            {
                if (this._log)
                    Logger.Log($"SOCKS [{client.Id}] : Disconnect from exception {e}.");
            }
            finally
            {
                packet = new Socks4Packet(client.Id, Socks4Packet.PacketType.DISCONNECT);
                _frameService.CacheFrame(AgentId, NetFrameType.Socks, packet);
                _socksClients.Remove(client.Id);
                client.Dispose();
            }
        }


        public async Task Stop()
        {
            _tokenSource.Cancel();
        }
    }
}