using FoxTail.Common;

namespace StargateNetwork.Worlds.Dummy;

public class DummyStargateWorldManager : IStargateWorldManager
{
    public bool IsWorldRunning(Uri recordUrl)
    {
        StargateServer.Instance.Logger.LogTrace(ResoCategory.StargateWorld, $"Server checking if '{recordUrl}' is running. Will return false.");
        return false;
    }

    public Task StartWorld(Uri recordUrl)
    {
        StargateServer.Instance.Logger.LogTrace(ResoCategory.StargateWorld, $"Server starting world '{recordUrl}'.");
        return Task.CompletedTask;
    }
}