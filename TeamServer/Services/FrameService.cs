using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Linq;
using System;
using Shared;
using System.Collections.Generic;
using Mono.Cecil;
using BinarySerializer;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TeamServer.Services;

public interface IFrameService
{
    byte[] GetData(NetFrame frame);
    //NetFrame CreateFrame(string source, string destination, NetFrameType typ, byte[] data);
    //NetFrame CreateFrame(string destination, NetFrameType typ, byte[] data);
    void AddCahedFrames(NetFrame frame);
    NetFrame CacheFrame(string source, string destination, NetFrameType typ, byte[] data);
    NetFrame CacheFrame(string destination, NetFrameType typ, byte[] data);

    NetFrame CacheFrame<T>(string source, string destination, NetFrameType typ, T item);
    NetFrame CacheFrame<T>(string destination, NetFrameType typ, T item);
    Queue<NetFrame> ExtractCachedFrame(string destination);
}

public class FrameService : IFrameService
{
    private Dictionary<string, Queue<NetFrame>> _CachedFrames = new Dictionary<string, Queue<NetFrame>>();

    private readonly ICryptoService _cryptoService;
    public string Key { get; private set; }
    public FrameService(ICryptoService cryptoService)
    {
        _cryptoService = cryptoService;
    }

    public void AddCahedFrames(NetFrame frame)
    {
        if(this._CachedFrames.ContainsKey(frame.Destination))
            this._CachedFrames[frame.Destination].Enqueue(frame);
        else
        {
            var q = new System.Collections.Generic.Queue<NetFrame>();
            q.Enqueue(frame);
            this._CachedFrames.Add(frame.Destination, q);
        }
    }

    public NetFrame CacheFrame(string source, string destination, NetFrameType typ, byte[] data)
    {
        var frame = CreateFrame(source, destination, typ, data);
        this.AddCahedFrames(frame);
        return frame;
    }

    public NetFrame CacheFrame<T>(string source, string destination, NetFrameType typ, T item)
    {
        var frame = CreateFrame(source, destination, typ, item);
        this.AddCahedFrames(frame);
        return frame;
    }
    public NetFrame CacheFrame<T>(string destination, NetFrameType typ, T item)
    {
        var frame = CreateFrame(destination, typ, item);
        this.AddCahedFrames(frame);
        return frame;
    }

    public NetFrame CacheFrame(string destination, NetFrameType typ, byte[] data)
    {
        var frame = CreateFrame(destination, typ, data);
        this.AddCahedFrames(frame);
        return frame;
    }

    public Queue<NetFrame> ExtractCachedFrame(string destination)
    {
        if(!this._CachedFrames.ContainsKey(destination))
            return new Queue<NetFrame>();

        var q = this._CachedFrames[destination];
        this._CachedFrames.Remove(destination);
        return q;
    }

    public NetFrame CreateFrame(string source, string destination, NetFrameType typ, byte[] data)
    {
        var newData = this._cryptoService.EncryptFrames ? this._cryptoService.Encrypt(data) : data;
        var frame = new NetFrame(source, destination, typ, newData);
        return frame;
    }

    public NetFrame CreateFrame<T>(string destination, NetFrameType typ, T item)
    {
        var data = item.BinarySerializeAsync().Result;
        return this.CreateFrame(string.Empty, destination, typ, data);
    }

    public NetFrame CreateFrame<T>(string source, string destination, NetFrameType typ, T item)
    {
        var data = item.BinarySerializeAsync().Result;
        var newData = this._cryptoService.EncryptFrames ? this._cryptoService.Encrypt(data) : data;
        var frame = new NetFrame(source, destination, typ, newData);
        return frame;
    }

    public NetFrame CreateFrame(string destination, NetFrameType typ, byte[] data)
    {
        return this.CreateFrame(string.Empty, destination, typ, data);
    }

    public byte[] GetData(NetFrame frame)
    {
        var data = frame.Data;
        return this._cryptoService.EncryptFrames ? this._cryptoService.Decrypt(data) : data;
    }
}