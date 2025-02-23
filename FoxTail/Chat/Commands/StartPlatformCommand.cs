using FoxTail.Chat.CommandSupport;
using FrooxEngine;

namespace FoxTail.Chat.Commands;

public class StartPlatformCommand : StartWorldPresetCommand
{
    protected override WorldAction Action => WorldPresets.SimplePlatform;
    public override string Name => "platform";
}