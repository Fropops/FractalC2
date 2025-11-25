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
    public sealed class RPortFwdClient : IDisposable
    {
        public string Id { get; private set; }
        public string AgentId { get; private set; }
        private TcpClient _tcp;
        private ConcurrentQueue<byte[]> _dataQueue = new ConcurrentQueue<byte[]>();
        public bool? ConnexionResult { get; private set; } = null;

        public RPortFwdClient(string id, string agentId)
        {
            this.Id = id;
            this.AgentId = agentId;
            this._tcp = new TcpClient();
        }

        public bool Connect(ReversePortForwardDestination dest)
        {
            try
            {
                this._tcp.Connect(dest.Hostname, dest.Port);
            }
            catch(Exception ex)
            {
                return false;
            }
            return true;
        }

        public void QueueData(byte[] data)
        {
            _dataQueue.Enqueue(data);
        }

        public bool TryDequeue(out byte[] data)
        {
            return _dataQueue.TryDequeue(out data);
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
            return this._tcp.Connected;
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

        public void Dispose()
        {
            if (this._tcp != null)
                this._tcp.Dispose();
        }
    }
}