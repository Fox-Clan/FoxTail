using FrooxEngine;
using NotEnoughLogs;

namespace JvyHeadlessRunner.EngineIntegration;

public class LoggerEngineInitProgress : IEngineInitProgress
{
    private readonly Logger _logger;
    private string _fixedPhase = "Initial";

    public LoggerEngineInitProgress(Logger logger)
    {
        _logger = logger;   
    }

    public void SetFixedPhase(string phase)
    {
        _fixedPhase = phase;
        ++FixedPhaseIndex;
        _logger.LogInfo(ResoCategory.EngineInit, $"-- {phase} --");
    }

    public void SetSubphase(string subphase, bool alwaysShow = false)
    {
        if (subphase == this._fixedPhase)
            return;

        _logger.LogInfo(ResoCategory.EngineInit, $"[{_fixedPhase}] {subphase}");
    }

    public void EngineReady()
    {
        _logger.LogInfo(ResoCategory.EngineInit, $"Engine is ready after {FixedPhaseIndex} phases");
    }

    public int FixedPhaseIndex { get; private set; }
}