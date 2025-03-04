using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace FoxTail.Common.Timing.ThrottleMethods;

[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
[SupportedOSPlatform("freebsd")]
public partial class UnixThrottleMethod : IThrottleMethod
{
    [LibraryImport("libc", SetLastError = true)]
    private static partial int nanosleep(in TimeSpec requested, out TimeSpec remaining);

    [StructLayout(LayoutKind.Sequential)]
    private struct TimeSpec
    {
        public long Seconds;
        public long NanoSeconds;
    }
    
    public bool Sleep(double ms)
    {
        const double nsPerMs = 1_000_000;
        const double msPerSecond = 1000;
        
        TimeSpec timeSpec = new()
        {
            Seconds = (long)(ms / msPerSecond),
            NanoSeconds = (long)((ms % nsPerMs) * nsPerMs)
        };

        // Console.WriteLine($"req: s:{timeSpec.Seconds} ns:{timeSpec.NanoSeconds}");
        int result = nanosleep(timeSpec, out TimeSpec _);
        return result == 0;
    }

    public bool Yield()
    {
        return Thread.Yield();
    }
}