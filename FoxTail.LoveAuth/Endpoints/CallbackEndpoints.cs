using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Protocols.Http;
using FoxTail.LoveAuth.Services;

namespace FoxTail.LoveAuth.Endpoints;

public class CallbackEndpoints : EndpointGroup
{
    [HttpEndpoint("/loveAuth/callback")]
    public Response Callback(RequestContext context, LoveAuthService service)
    {
        string? token = context.QueryString.Get("RLToken");
        if (token == null)
            return new Response("Token is missing!");

        service.ValidateJwt(token);
        
        Console.WriteLine(token);
        return new Response("OK");
    }
}