using System.Reflection;
using FoxTail.Common;
using FoxTail.LoveAuth.Services;

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