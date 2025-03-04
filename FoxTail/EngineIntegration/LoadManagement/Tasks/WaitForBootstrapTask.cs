﻿using System.Diagnostics;
using FoxTail.Common;

namespace FoxTail.EngineIntegration.LoadManagement.Tasks;

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
    public override async Task ExecuteAsync(HeadlessContext context)
    {
        Stopwatch sw = Stopwatch.StartNew();
        while (!Bootstrapped)
        {
            if (sw.ElapsedMilliseconds > 30_000)
                throw new Exception("Userspace never bootstrapped, waited for 30s");
            
            await Task.Delay(25);
        }
        
        context.Logger.LogInfo(ResoCategory.EngineInit, $"Userspace has been bootstrapped! Took ~{sw.ElapsedMilliseconds}ms.");
    }

    public void Dispose()
    {
        Instance = null;
        GC.SuppressFinalize(this);
    }
}