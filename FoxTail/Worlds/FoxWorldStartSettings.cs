using FrooxEngine;

namespace FoxTail.Worlds;

public class FoxWorldStartSettings : WorldStartSettings
{
    public string FriendlyName = "Automatic World Startup";
    public string? OverrideName;
    public IEnumerable<string> InviteUsernames = [];
    public string? InviteMessage;
}