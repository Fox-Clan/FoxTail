using System.Reflection;
using FoxTail.Common;
using FoxTail.LoveAuth.Services;

namespace FoxTail.LoveAuth;

public static class FoxLoveAuth
{
    public static void SetupBunkum()
    {
        FoxBunkumServer.RegisterSetupAction(s =>
        {
            s.AddService<LoveAuthService>();
            s.DiscoverEndpointsFromAssembly(Assembly.GetExecutingAssembly());
        });
    }
}