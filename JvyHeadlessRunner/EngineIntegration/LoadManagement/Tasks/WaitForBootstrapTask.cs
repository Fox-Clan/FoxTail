using System.Diagnostics;
using FrooxEngine;
using NotEnoughLogs;

namespace JvyHeadlessRunner.EngineIntegration.LoadManagement.Tasks;

public class WaitForBootstrapTask : InitTask, IDisposable
{
    public static WaitForBootstrapTask? Instance;
    
    public bool Bootstrapped = false;

    public WaitForBootstrapTask()
    {
        Instance = this;
    }
    
    public override string Name => "Wait for Userspace Bootstrap";
    public override InitTaskStage Stage => InitTaskStage.Immediate;
    public override async Task ExecuteAsync(Logger logger, Engine engine)
    {
        Stopwatch sw = Stopwatch.StartNew();
        while (!Bootstrapped)
        {
            if (sw.ElapsedMilliseconds > 30_000)
                throw new Exception("Userspace never bootstrapped, waited for 30s");
            
            await Task.Delay(25);
        }
        
        logger.LogInfo(ResoCategory.EngineInit, $"Userspace has been bootstrapped! Took ~{sw.ElapsedMilliseconds}ms.");
    }

    public void Dispose()
    {
        Instance = null;
        GC.SuppressFinalize(this);
    }
}