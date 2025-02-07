using FrooxEngine;
using NotEnoughLogs;

namespace JvyHeadlessRunner.EngineIntegration;

public class LoggerEngineInitProgress : IEngineInitProgress
{
    private readonly HeadlessContext _context;
    private string _fixedPhase = "Initial";

    private bool _ready;

    public LoggerEngineInitProgress(HeadlessContext context)
    {
        this._context = context;   
    }

    public void SetFixedPhase(string phase)
    {
        if (this._ready)
            return;

        this._fixedPhase = phase;
        ++this.FixedPhaseIndex;
        this._context.Logger.LogInfo(ResoCategory.EngineInit, $"-- {phase} --");
    }

    public void SetSubphase(string subphase, bool alwaysShow = false)
    {
        if (this._ready)
            return;
        
        if (subphase == this._fixedPhase)
            return;

        this._context.Logger.LogInfo(ResoCategory.EngineInit, $"[{_fixedPhase}] {subphase}");
    }

    public void EngineReady()
    {
        this._context.Logger.LogInfo(ResoCategory.EngineInit, $"Engine is ready after {FixedPhaseIndex} phases!");
        this._ready = true;
        this.SetFixedPhase("Running");
    }

    public int FixedPhaseIndex { get; private set; }
}