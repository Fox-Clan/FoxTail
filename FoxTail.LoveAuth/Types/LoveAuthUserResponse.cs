using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FoxTail.LoveAuth.Types;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class LoveAuthUserResponse
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string ResoniteUserId { get; set; }
    public string? DiscordId { get; set; }
    public string? MisskeyId { get; set; }
    
    [JsonProperty("aud")]
    public string Audience { get; set; }
    [JsonProperty("exp")]
    public long Expiration { get; set; }
}