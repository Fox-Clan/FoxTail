using System.Diagnostics;
using System.Runtime.InteropServices;
using Elements.Assets;
using FoxTail.Common;
using FoxTail.Common.Timing;
using FrooxEngine;
using Hardware.Info;

namespace FoxTail.EngineIntegration;

public class FoxSystemInfo : ISystemInfo
{
    private readonly HeadlessContext _context;
    private readonly ThrottledClock _clock;
    
    public FoxSystemInfo(HeadlessContext context, ThrottledClock clock)
    {
        this._context = context;
        this._clock = clock;
        
        context.Logger.LogInfo(ResoCategory.EngineInit, "Gathering hardware info...");
        
        HardwareInfo info = new();
        info.RefreshOperatingSystem();
        info.RefreshCPUList(false);
        info.RefreshMemoryStatus();

        this.OperatingSystem = info.OperatingSystem.ToString();
        this.CPU = info.CpuList.First().Name;
        this.PhysicalCores = info.CpuList.Sum(c => (int)c.NumberOfCores);
        this.GPU = "N/A";
        this.VRAMBytes = 0;

        this.MemoryBytes = (long)info.MemoryStatus.TotalPhysical;
        
        context.Logger.LogDebug(ResoCategory.EngineInit, $"Throttle method: {clock.ThrottleMethodName}");
    }

    public Platform Platform
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return Platform.Linux;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return Platform.Windows;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return Platform.OSX;

            return Platform.Other;
        }
    }

    public HeadOutputDevice HeadDevice => HeadOutputDevice.Headless;
    public string UniqueDeviceIdentifier => null!;
    public bool IsAOT => false;
    public int MaxTextureSize => 16384;
    public string XRDeviceName => "FoxTail Headless";
    public string XRDeviceModel => "FoxTail Headless";
    public bool IsGPUTexturePOTByteAligned => true;
    public bool UsingLinearSpace => false;
    
    public string OperatingSystem { get; }
    public string CPU { get; }
    public string GPU { get; }
    public int? PhysicalCores { get; }
    public long MemoryBytes { get; }
    public long VRAMBytes { get; }
    public string StereoRenderingMode { get; } = null!;
    
    public bool SupportsTextureFormat(TextureFormat format) => true;

    public void RegisterThread(string name)
    {
    }

    public void BeginSample(string name)
    {
    }

    public void EndSample()
    {
    }
    
    public float FPS { get; private set; }
    public float ImmediateFPS => FPS;
    
    // we misuse this to measure jitter so it shows up in game
    public float RenderTime { get; private set; }
    public float ExternalUpdateTime => 0;
    
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    /// <summary>
    /// How many exceptions we can handle before crashing
    /// </summary>
    private const int MaxLives = 3;
    /// <summary>
    /// How many lives we have remaining
    /// </summary>
    private int _lives = MaxLives;

    /// <summary>
    /// 
    /// </summary>
    /// <returns>True if we should crash. False if we can try to resume again</returns>
    public bool HandleException()
    {
        _lives--;
        return _lives <= 0;
    }

    public void FrameFinished()
    {
        this.FPS = this._clock.FramesPerSecond;
        this.RenderTime = this._clock.Jitter;
        
        if (this._stopwatch.Elapsed.TotalSeconds < 1.0)
            return;

        // rest of function runs every second rather than every frame
        
        this._stopwatch.Restart();

        if (_lives < MaxLives)
            _lives++;
        
        if(_context.Config.LogUpdatesPerSecond)
            this._context.Logger.LogDebug(ResoCategory.Runner, this._clock.ToString());
    }
}