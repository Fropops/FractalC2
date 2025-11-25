using System.Threading.Tasks;
using BinarySerializer;
using Shared;
using TeamServer.Services;

namespace TeamServer.FrameHandling;

public abstract class FrameHandler
{
    public IServerService Server { get; protected set; }

    public void Init(IServerService server)
    {
        this.Server = server;
    }
    public abstract NetFrameType FrameType { get; }

    public abstract Task ProcessFrame(NetFrame frame, string relay);

    protected async Task<T> ExtractFrameData<T>(NetFrame frame)
    {
        var data = this.Server.FrameService.GetData(frame);
        return await data.BinaryDeserializeAsync<T>();
    }
}