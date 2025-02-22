namespace StargateNetwork;

public class StargateConfiguration
{
    public bool StargateServerIntegration { get; set; } = false;
    public string WebsocketHostUrl { get; set; } = "ws://127.0.0.1:10060";
    public bool WebsocketEnabled { get; set; } = true;
    public bool BunkumEnabled { get; set; } = true;
}