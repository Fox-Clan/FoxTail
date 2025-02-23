using FoxTail.Chat.Platforms;
using JetBrains.Annotations;

namespace FoxTail.Chat.CommandSupport;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface IChatCommand
{
    public string Name { get; }
    public string HelpText { get; }
    public Task InvokeAsync(HeadlessContext context, IChatChannel channel, IChatUser user, ArgContainer args);
}