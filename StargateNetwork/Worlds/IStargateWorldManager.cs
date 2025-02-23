namespace StargateNetwork.Worlds;

public interface IStargateWorldManager
{
    public bool IsWorldRunning(Uri recordUrl);
    public Task StartWorld(Uri recordUrl);
}