namespace FoxTail.Common.Timing.ThrottleMethods;

public interface IThrottleMethod
{
    /// <summary>
    /// Sleep for, at minimum, <c>ms</c> milliseconds.
    /// </summary>
    /// <param name="ms">The number of milliseconds to sleep for.</param>
    bool Sleep(double ms);
    
    /// <summary>
    /// Hint to the OS scheduler that we are ready to relinquish control to other threads.
    /// </summary>
    /// <returns>True if we gave our timeslice away to another thread, false if we kept running.</returns>
    bool Yield();
}