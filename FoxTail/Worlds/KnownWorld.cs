using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using SkyFrost.Base;

namespace FoxTail.Worlds;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class KnownWorld
{
    public string KnownWorldId { get; set; } = "example";
    public string? OverrideName { get; set; }

    public bool StartWithServer { get; set; } = false;

    public string? InviteMessage { get; set; } = "Come join my world!";
    public IEnumerable<string> InviteUsernames { get; set; } = [];

    [JsonConverter(typeof(StringEnumConverter))]
    public SessionAccessLevel AccessLevel { get; set; } = SessionAccessLevel.Contacts;
    public bool HideFromListing { get; set; } = false;

    public string RecordUrl { get; set; } = "resrec:///Owner/ID";

    public FoxWorldStartSettings Compile()
    {
        return new FoxWorldStartSettings
        {
            FriendlyName = this.KnownWorldId,
            OverrideName = this.OverrideName,
            InviteMessage = this.InviteMessage,
            InviteUsernames = this.InviteUsernames,
            DefaultAccessLevel = this.AccessLevel,
            HideFromListing = this.HideFromListing,
            URIs = [new Uri(this.RecordUrl)]
        };
    }
}