using FrooxEngine;
using JetBrains.Annotations;
using NotEnoughLogs;

namespace JvyHeadlessRunner.EngineIntegration.LoadManagement;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract class InitTask
{
    public abstract string Name { get; }
    public abstract InitTaskStage Stage { get; }
    public abstract Task ExecuteAsync(Logger logger, Engine engine);
}