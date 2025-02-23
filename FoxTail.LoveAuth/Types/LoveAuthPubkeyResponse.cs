using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FoxTail.LoveAuth.Types;

#nullable disable

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class LoveAuthPubkeyResponse
{
    public bool Success { get; set; }
    public string Key { get; set; }
    [JsonProperty("alg")]
    public string Algorithm { get; set; }
}