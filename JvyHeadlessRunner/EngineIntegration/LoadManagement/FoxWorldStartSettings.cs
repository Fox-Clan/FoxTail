using FrooxEngine;
using JetBrains.Annotations;

namespace JvyHeadlessRunner.EngineIntegration.LoadManagement;

public class FoxWorldStartSettings : WorldStartSettings
{
    public string FriendlyName = "Automatic World Startup";
    public string? OverrideName;
    public IEnumerable<string> InviteUsernames = [];
    public string? InviteMessage;
}