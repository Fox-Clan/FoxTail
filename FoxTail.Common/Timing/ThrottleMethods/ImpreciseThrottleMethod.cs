namespace FoxTail.Common.Timing.ThrottleMethods;

/// <summary>
/// A non high-precision throttle method.
/// Can have inaccuracy up to ~16ms on Windows.
/// Source: https://learn.microsoft.com/en-us/archive/blogs/ericlippert/precision-and-accuracy-of-datetime
/// </summary>
public class ImpreciseThrottleMethod : IThrottleMethod
{
    /// <inheritdoc/>
    public bool Sleep(double ms)
    {
        Thread.Sleep(TimeSpan.FromMilliseconds(ms));
        return true;
    }

    /// <inheritdoc/>
    public bool Yield()
    {
        Thread.Sleep(0);
        return true;
    }
}