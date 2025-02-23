using System.Net;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using FoxTail.LoveAuth.Services;
using FoxTail.LoveAuth.Types;
using JWT.Exceptions;

namespace FoxTail.LoveAuth.Endpoints;

public class CallbackEndpoints : EndpointGroup
{
    [HttpEndpoint("/loveAuth/callback")]
    public Response Callback(RequestContext context, LoveAuthService service)
    {
        string? token = context.QueryString.Get("RLToken");
        if (token == null)
            return new Response("Token is missing!", ContentType.Plaintext, HttpStatusCode.BadRequest);

        LoveAuthUserResponse? user = null;
        try
        {
            user = service.ValidateJwt(token);
            if (user == null)
                return new Response("Invalid user/missing authentication", ContentType.Plaintext, HttpStatusCode.Forbidden);
        }
        catch (SignatureVerificationException e)
        {
            return new Response(e.Message, ContentType.Plaintext, HttpStatusCode.Unauthorized);
        }

        if (user.DiscordId == null)
            return new Response("Please link your Discord account at https://auth.resonite.love!", ContentType.Plaintext, HttpStatusCode.Unauthorized);
        
        Console.WriteLine($"{user.ResoniteUserId}'s Discord id is '" + user.DiscordId + "'!");
        
        return new Response("Success! You may now close this tab.");
    }
}