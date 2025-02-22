using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Newtonsoft.Json;

namespace StargateNetwork.endpoints;

public class GateEndpoints : EndpointGroup
{
    //returns gatelist for ingame applications
    [HttpEndpoint("/gates", HttpMethods.Get, ContentType.Plaintext)]
    public String GetGates(RequestContext context)
    {
        using (var db = new StargateContext())
        {
            var gates = StargateTools.FindAllGates(db, onlyNonPersistent: false, onlyPublic: true);
            string gateList = JsonConvert.SerializeObject(gates);
            return gateList;
        }
    }
}