using System.Runtime.Versioning;

namespace FoxTail.Common.Timing.ThrottleMethods;

[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
[SupportedOSPlatform("freebsd")]
public class UnixThrottleMethod : IThrottleMethod
{
    public bool Sleep(double ms)
    {
        throw new NotImplementedException();
    }

    public bool Yield()
    {
        return Thread.Yield();
    }
}