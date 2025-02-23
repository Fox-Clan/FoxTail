using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Protocols.Http;

namespace FoxTail.LoveAuth.Endpoints;

public class CallbackEndpoints : EndpointGroup
{
    [HttpEndpoint("/loveAuth/callback")]
    public Response Callback(RequestContext context)
    {
        string? token = context.QueryString.Get("RLToken");
        if (token == null)
            return new Response("Token is missing!");
        
        Console.WriteLine(token);
        return new Response("OK");
    }
}