using FoxTail.Common;

namespace FoxTail.LoveAuth;

internal static class Program
{
    internal static async Task Main(string[] args)
    {
        FoxLoveAuth.SetupBunkum();
        FoxBunkumServer.Start();
        await Task.Delay(-1);
    }
}