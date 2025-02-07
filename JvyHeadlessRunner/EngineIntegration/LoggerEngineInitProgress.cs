using FrooxEngine;
using NotEnoughLogs;

namespace JvyHeadlessRunner.EngineIntegration;

public class LoggerEngineInitProgress : IEngineInitProgress
{
    private readonly Logger _logger;
    private string _fixedPhase = "Initial";

    private bool _ready;

    public LoggerEngineInitProgress(Logger logger)
    {
        this._logger = logger;   
    }

    public void SetFixedPhase(string phase)
    {
        if (this._ready)
            return;

        this._fixedPhase = phase;
        ++this.FixedPhaseIndex;
        this._logger.LogInfo(ResoCategory.EngineInit, $"-- {phase} --");
    }

    public void SetSubphase(string subphase, bool alwaysShow = false)
    {
        if (this._ready)
            return;
        
        if (subphase == this._fixedPhase)
            return;

        this._logger.LogInfo(ResoCategory.EngineInit, $"[{_fixedPhase}] {subphase}");
    }

    public void EngineReady()
    {
        this._logger.LogInfo(ResoCategory.EngineInit, $"Engine is ready after {FixedPhaseIndex} phases!");
        this._ready = true;
        this.SetFixedPhase("Running");
    }

    public int FixedPhaseIndex { get; private set; }
}