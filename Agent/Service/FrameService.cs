using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinarySerializer;
using Shared;

namespace Agent.Service
{
    internal interface IFrameService
    {
        NetFrame CreateFrame<T>(string source, string destination, NetFrameType typ, T item);
        NetFrame CreateFrame(string source, string destination, NetFrameType typ, byte[] data);

        NetFrame CreateFrame<T>(string source, NetFrameType typ, T item);
        NetFrame CreateFrame(string source, NetFrameType typ, byte[] data);

        byte[] GetData(NetFrame frame);
        T GetData<T>(NetFrame frame);
    }
    internal class FrameService : IFrameService
    {
        private readonly ICryptoService _cryptoService;
        private readonly IConfigService _configService;
        public FrameService(ICryptoService cryptoService, IConfigService configService)
        {
            _cryptoService = cryptoService;
            _configService = configService;
        }

        public NetFrame CreateFrame(string source, string destination, NetFrameType typ, byte[] data)
        {
            var newData = this._configService.EncryptFrames ? this._cryptoService.Encrypt(data) : data;
            var frame = new NetFrame(source, destination, typ, newData);
            return frame;
        }

        public NetFrame CreateFrame(string source, NetFrameType typ, byte[] data)
        {
            return this.CreateFrame(source, String.Empty, typ, data);
        }

        public byte[] GetData(NetFrame frame)
        {
            var data = frame.Data;
            return this._configService.EncryptFrames ? this._cryptoService.Decrypt(data) : data;
        }

        public T GetData<T>(NetFrame frame)
        {
            var raw = GetData(frame);
            return raw.BinaryDeserializeAsync<T>().Result;
        }

        public NetFrame CreateFrame<T>(string source, string destination, NetFrameType typ, T item)
        {
            return this.CreateFrame(source, destination, typ, item.BinarySerializeAsync().Result);
        }

        public NetFrame CreateFrame<T>(string source, NetFrameType typ, T item)
        {
            return this.CreateFrame(source, string.Empty, typ, item);
        }
    }

}
