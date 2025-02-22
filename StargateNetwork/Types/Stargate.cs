using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace StargateNetwork.Types;

#nullable disable

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
[SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength")]
public class Stargate
{
    public int ActiveUsers { get; set; }
    public string GateAddress { get; set; }
    public string GateCode { get; set; }
    public string GateStatus { get; set; }
    public string Id { get; set; }
    public string IrisState { get; set; }
    public bool IsHeadless { get; set; }
    public int MaxUsers { get; set; }
    public string OwnerName { get; set; }
    public bool PublicGate { get; set; }
    public string SessionName { get; set; }
    public string SessionUrl { get; set; }
    public long UpdateDate { get; set; }
    public long CreationDate { get; set; }
    public string DialedGateId { get; set; }
    public bool IsPersistent { get; set; }
    public string WorldRecord { get; set; }
}