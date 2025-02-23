using FoxTail.Chat.CommandSupport;
using FrooxEngine;

namespace FoxTail.Chat.Commands;

public class StartGridCommand : StartWorldPresetCommand
{
    protected override WorldAction Action => WorldPresets.Grid;
    public override string Name => "grid";
}