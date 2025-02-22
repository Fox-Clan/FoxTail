using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FoxTail.Users;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class KnownUser
{
    public string Name { get; set; } = "John Resonite";
    public IEnumerable<string> Ids { get; set; } = [];
}