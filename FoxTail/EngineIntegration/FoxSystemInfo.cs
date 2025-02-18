using System.Diagnostics;
using System.Runtime.InteropServices;
using Elements.Assets;
using FrooxEngine;
using Hardware.Info;

namespace FoxTail.EngineIntegration;

public class FoxSystemInfo : ISystemInfo
{
    private HeadlessContext _context;
    
    public FoxSystemInfo(HeadlessContext context)
    {
        this._context = context;
        
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
    public string UniqueDeviceIdentifier => null;
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
    public string StereoRenderingMode { get; } = null;
    
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
    public float RenderTime => 0;
    public float ExternalUpdateTime => 0;
    
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private int _frames;

    public void FrameFinished()
    {
        ++this._frames;
        
        if (this._stopwatch.Elapsed.TotalSeconds < 1.0)
            return;
        
        this.FPS = this._frames / (float) this._stopwatch.Elapsed.TotalSeconds;
        
        this._stopwatch.Restart();
        this._frames = 0;
        
        if(_context.Config.LogUpdatesPerSecond)
            this._context.Logger.LogDebug(ResoCategory.Runner, $"Engine UPS: {FPS:N2}");
    }
}