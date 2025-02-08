using JetBrains.Annotations;

namespace FoxTail.EngineIntegration.LoadManagement;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract class InitTask
{
    public abstract string Name { get; }
    public abstract InitTaskStage Stage { get; }
    public abstract Task ExecuteAsync(HeadlessContext context);
}