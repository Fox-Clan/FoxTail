using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FoxTail.Configuration;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class FoxTailConfig
{
    public float TickRate { get; set; } = 60.0f;
    public IEnumerable<string> AllowedHttpHosts { get; set; } = [];
    public IEnumerable<string> AllowedWebsocketHosts { get; set; } = [];
    public IEnumerable<string> AllowedOSCSenderHosts { get; set; } = [];
    public IEnumerable<int> AllowedOSCReceiverPorts { get; set; } = [];
}