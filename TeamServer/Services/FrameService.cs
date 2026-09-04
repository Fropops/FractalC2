using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Shared;
using System.Collections.Generic;
using Mono.Cecil;
using BinarySerializer;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TeamServer.Services;

[InjectableService]
public interface IFrameService
{
    byte[] GetData(NetFrame frame);
    //NetFrame CreateFrame(string source, string destination, NetFrameType typ, byte[] data);
    //NetFrame CreateFrame(string destination, NetFrameType typ, byte[] data);
    void AddCachedFrames(NetFrame frame);
    NetFrame CacheFrame(string source, string destination, NetFrameType typ, byte[] data);
    NetFrame CacheFrame(string destination, NetFrameType typ, byte[] data);

    Task<NetFrame> CacheFrameAsync<T>(string source, string destination, NetFrameType typ, T item);
    Task<NetFrame> CacheFrameAsync<T>(string destination, NetFrameType typ, T item);

    Task<NetFrame> CacheCheckInFrameAsync(string destination);
    Queue<NetFrame> ExtractCachedFrame(string destination);
}

[InjectableServiceImplementation(typeof(IFrameService))]
public class FrameService : IFrameService
{
    private readonly ConcurrentDictionary<string, ConcurrentQueue<NetFrame>> _CachedFrames = new();

    private readonly ICryptoService _cryptoService;
    public string Key { get; private set; }
    public FrameService(ICryptoService cryptoService)
    {
        _cryptoService = cryptoService;
    }

    public void AddCachedFrames(NetFrame frame)
    {
        var q = _CachedFrames.GetOrAdd(frame.Destination, _ => new ConcurrentQueue<NetFrame>());
        q.Enqueue(frame);
    }

    public NetFrame CacheFrame(string source, string destination, NetFrameType typ, byte[] data)
    {
        var frame = CreateFrame(source, destination, typ, data);
        this.AddCachedFrames(frame);
        return frame;
    }

    public async Task<NetFrame> CacheCheckInFrameAsync(string destination)
    {
        var task = new AgentTask()
        {
            Id = Guid.NewGuid().ToString(),
            CommandId = CommandId.CheckIn,
        };
        return await this.CacheFrameAsync(destination, NetFrameType.Task, task);
    }

    public async Task<NetFrame> CacheFrameAsync<T>(string source, string destination, NetFrameType typ, T item)
    {
        var frame = await CreateFrameAsync(source, destination, typ, item);
        this.AddCachedFrames(frame);
        return frame;
    }
    public async Task<NetFrame> CacheFrameAsync<T>(string destination, NetFrameType typ, T item)
    {
        var frame = await CreateFrameAsync(destination, typ, item);
        this.AddCachedFrames(frame);
        return frame;
    }

    public NetFrame CacheFrame(string destination, NetFrameType typ, byte[] data)
    {
        var frame = CreateFrame(destination, typ, data);
        this.AddCachedFrames(frame);
        return frame;
    }

    public Queue<NetFrame> ExtractCachedFrame(string destination)
    {
        if (_CachedFrames.TryRemove(destination, out var q))
            return new Queue<NetFrame>(q);
        return new Queue<NetFrame>();
    }

    public NetFrame CreateFrame(string source, string destination, NetFrameType typ, byte[] data)
    {
        var newData = this._cryptoService.EncryptFrames ? this._cryptoService.Encrypt(data) : data;
        var frame = new NetFrame(source, destination, typ, newData);
        return frame;
    }

    public async Task<NetFrame> CreateFrameAsync<T>(string destination, NetFrameType typ, T item)
    {
        var data = await item.BinarySerializeAsync();
        return this.CreateFrame(string.Empty, destination, typ, data);
    }

    public async Task<NetFrame> CreateFrameAsync<T>(string source, string destination, NetFrameType typ, T item)
    {
        var data = await item.BinarySerializeAsync();
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