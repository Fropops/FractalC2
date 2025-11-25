using Agent.Communication;
using Agent.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Agent.Helpers;
using System.Text;
using BinarySerializer;
using Shared;
using System.Security.Principal;
using System.Security.AccessControl;
using MiscUtil.Conversion;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;

namespace Agent.Models
{
    public class TcpCommModule : P2PCommunicator
    {
        private static bool Loopback => false;

        public TcpCommModule(ConnexionUrl conn) : base(conn)
        {
            if (conn.Protocol == ConnexionType.Tcp)
            {
                CommunicationMode = CommunicationModuleMode.Server;
                return;
            }
            if (conn.Protocol == ConnexionType.ReverseTcp)
            {
                CommunicationMode = CommunicationModuleMode.Client;
                return;
            }

            throw new ArgumentException($"{conn.Protocol} is not a valid protocol.");
        }

        private TcpListener _listener;
        private TcpClient _client;

        public override event Func<NetFrame, Task> FrameReceived;
        public override event Action OnException;

        public override void Init(Agent agent)
        {
            base.Init(agent);
            _tokenSource = new CancellationTokenSource();
        }

        public override async Task Start()
        {
            switch (this.CommunicationMode)
            {
                case CommunicationModuleMode.Server:
                    {
                        var address = this.Connexion.IsLoopBack ? IPAddress.Loopback : IPAddress.Any;
                        _listener = new TcpListener(new IPEndPoint(address, this.Connexion.Port));
                        _listener.Start(100);

                        break;
                    }

                case CommunicationModuleMode.Client:
                    {
                        _client = new TcpClient();

                        break;
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (this.CommunicationMode)
            {
                case CommunicationModuleMode.Server:
                    {
                        // wait for client
                        _client = await _listener.AcceptTcpClientAsync();

                        // once connected, stop listening
                        _listener.Stop();
#if DEBUG
                        Debug.WriteLine("TCP : Comm connected (server mode)");
#endif
                        break;
                    }

                case CommunicationModuleMode.Client:
                    {
                        await _client.ConnectAsync(this.Connexion.Address, this.Connexion.Port);
#if DEBUG
                        Debug.WriteLine("Tcp : Comm connected (client mode)");
#endif
                        break;
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }

            this.IsRunning = true;

        }

        public override async Task Run()
        {
         
            while (!_tokenSource.IsCancellationRequested)
            {
                try
                {
                    //#if DEBUG
                    //                    Debug.WriteLine($"Tcp : Read Loop");
                    //#endif
                    if (!_client.IsAlive())
                        throw new Exception();

                    if (_client.DataAvailable())
                    {

                        var data = await this.ReadStream(_client.GetStream());

                        var frame = await data.BinaryDeserializeAsync<NetFrame>();

#if DEBUG
                        //                        var base64 = Convert.ToBase64String(data);
                        //                        Debug.WriteLine($"Tcp : Received Frame(s) : {base64}");
                        Debug.WriteLine($"Tcp : Received Frame(s) : {frame.FrameType}");
#endif
                        
                        await this.FrameReceived?.Invoke(frame);

                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    Debug.WriteLine($"Tcp : Error reading pipe : {ex}");
#endif
                    this.OnException?.Invoke();
                    return;
                }

                await Task.Delay(100);
            }

#if DEBUG
            Debug.WriteLine($"Tcp : Closing");
#endif

            _client?.Dispose();
            _tokenSource.Dispose();
        }

        public override async Task SendFrame(NetFrame frame)
        {
            try
            {
                var data = await frame.BinarySerializeAsync();
                var stream = _client.GetStream();
                await this.WriteStream(stream, data);
            }
            catch
            {
                OnException?.Invoke();
            }
        }

        object writeLock = new object();

        private async Task WriteStream(Stream stream, byte[] data)
        {
            lock (writeLock)
            {
//#if DEBUG
//                Debug.WriteLine($"Pipe : Send Length : {data.Length}");
//#endif
                // format data as [length][value]
                var lengthBuf = new BigEndianBitConverter().GetBytes(data.Length);
                stream.Write(lengthBuf, 0, lengthBuf.Length);

                using (var ms = new MemoryStream(data))
                {


                    // write in chunks
                    var bytesRemaining = data.Length;
                    do
                    {
                        var lengthToSend = bytesRemaining < 1024 ? bytesRemaining : 1024;
//#if DEBUG
//                        Debug.WriteLine($"Pipe : Write : {lengthToSend} / {bytesRemaining} / {data.Length}");
//#endif
                        var buf = new byte[lengthToSend];

                        var read = ms.Read(buf, 0, lengthToSend);

                        if (read != lengthToSend)
                            throw new Exception("Could not read data from stream");

                        stream.Write(buf, 0, buf.Length);

                        bytesRemaining -= lengthToSend;
                    }
                    while (bytesRemaining > 0);
                }
            }
        }

        private async Task<byte[]> ReadStream(Stream stream)
        {
            // read length
            var lengthBuf = new byte[4];
            var read = await stream.ReadAsync(lengthBuf, 0, 4);

            if (read != 4)
                throw new Exception("Failed to read length");

            var length = new BigEndianBitConverter().ToInt32(lengthBuf, 0);

//#if DEBUG
//            Debug.WriteLine($"Pipe : Received Length : {length}");
//#endif

            // read rest of data
            using (var ms = new MemoryStream())
            {
                var totalRead = 0;

                do
                {
                    try
                    {
                        var buf = length - totalRead >= 1024 ? new byte[1024] : new byte[length - totalRead];
//#if DEBUG
//                        Debug.WriteLine($"Pipe : Read : {buf.Length} / {totalRead} / {length}");
//#endif


                        read = await stream.ReadAsync(buf, 0, buf.Length);

                        await ms.WriteAsync(buf, 0, read);
                        totalRead += read;
                    }
                    catch (Exception ex)
                    {
                        int i = 0;
                    }

                }
                while (totalRead < length);

                return ms.ToArray();
            }
        }

    }
}
