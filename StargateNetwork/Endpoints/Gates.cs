using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using StargateNetwork.Types;

namespace StargateNetwork.Endpoints;

public class GateEndpoints : EndpointGroup
{
    //returns gatelist for ingame applications
    [HttpEndpoint("/gates", HttpMethods.Get, ContentType.Json)]
    public IEnumerable<Stargate> GetGates(RequestContext context)
    {
        using StargateContext db = new();
        return db.FindAllGates(onlyNonPersistent: false, onlyPublic: true);
    }
}