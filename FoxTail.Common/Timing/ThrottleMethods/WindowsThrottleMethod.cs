using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Versioning;

namespace FoxTail.Common.Timing.ThrottleMethods;

/// <summary>
/// A high-precision throttling method that uses Windows' high resolution wait timers.
/// Only works since Windows 10 v1803.
/// </summary>
[SupportedOSPlatform("windows")]
public partial class WindowsThrottleMethod : IThrottleMethod, IDisposable
{
    #region Native Interop
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [SupportedOSPlatform("windows")]
    private static partial IntPtr CreateWaitableTimerExW(IntPtr lpTimerAttributes, [MarshalAs(UnmanagedType.LPWStr)] string? lpTimerName, uint dwFlags, uint dwDesiredAccess);
    
    [DllImport("kernel32.dll", SetLastError = true)]
    [SupportedOSPlatform("windows")]
    [return:MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWaitableTimerEx(IntPtr hTimer, in FILETIME lpDueTime, int lPeriod, IntPtr routine, IntPtr lpArgToCompletionRoutine, IntPtr reason, uint tolerableDelay);
    
    [LibraryImport("kernel32.dll")]
    [SupportedOSPlatform("windows")]
    private static partial uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);
    
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [SupportedOSPlatform("windows")]
    [return:MarshalAs(UnmanagedType.Bool)]
    private static partial bool CloseHandle(IntPtr handle);
    
    private static FILETIME CreateFileTime(double ms)
    {
        TimeSpan ts = TimeSpan.FromMilliseconds(ms);
        ulong ul = unchecked((ulong)-ts.Ticks);
        return new FILETIME { dwHighDateTime = (int)(ul >> 32), dwLowDateTime = (int)(ul & 0xFFFFFFFF) };
    }

    private const uint TIMER_MANUAL_RESET = 0x00000001;
    private const uint TIMER_HIGH_RESOLUTION = 0x00000002;
    
    private const uint EVENT_ALL_ACCESS = 0x1F0003;
    #endregion

    private readonly IntPtr _timerHandle;

    public WindowsThrottleMethod()
    {
        this._timerHandle = CreateWaitableTimerExW(IntPtr.Zero, null,  TIMER_MANUAL_RESET | TIMER_HIGH_RESOLUTION, EVENT_ALL_ACCESS);
        if (this._timerHandle == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }
    
    /// <inheritdoc/>
    public bool Sleep(double ms)
    {
        if (this._timerHandle == IntPtr.Zero)
            return false;
        
        if (SetWaitableTimerEx(this._timerHandle, CreateFileTime(ms), 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0))
        {
            uint waitRes = WaitForSingleObject(this._timerHandle, uint.MaxValue);
            if (waitRes != 0)
                throw new Exception($"WaitForSingleObject Failed: 0x{waitRes:X8} (see https://learn.microsoft.com/en-us/windows/win32/api/synchapi/nf-synchapi-waitforsingleobject#return-value for meaning)");
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public bool Yield()
    {
        return Thread.Yield();
    }

    public void Dispose()
    {
        if(this._timerHandle != IntPtr.Zero)
            CloseHandle(this._timerHandle);

        GC.SuppressFinalize(this);
    }
}