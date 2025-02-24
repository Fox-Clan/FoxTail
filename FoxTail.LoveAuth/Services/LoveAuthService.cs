using System.Net.Http.Headers;
using Bunkum.Core.Services;
using FoxTail.Common;
using FoxTail.LoveAuth.Crypto;
using FoxTail.LoveAuth.Types;
using JWT.Builder;
using JWT.Exceptions;
using Newtonsoft.Json;
using NotEnoughLogs;

namespace FoxTail.LoveAuth.Services;

public class LoveAuthService : EndpointService
{
    private readonly HttpClient _client = new()
    {
        BaseAddress = new Uri("https://auth.resonite.love/"),
        DefaultRequestHeaders = { UserAgent = { new ProductInfoHeaderValue("FoxTail", "1.0") }}
    };

    public LoveAuthService(Logger logger) : base(logger)
    {}

    private T Get<T>(string endpoint)
    {
        HttpResponseMessage response = this._client.GetAsync(endpoint).Result;
        response.EnsureSuccessStatusCode();

        T? resp = JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
        if (resp == null)
            throw new Exception("Response failed to parse; was null");
        return resp;
    }

    public override void Initialize()
    {
        base.Initialize();
        
        this.Logger.LogInfo(ResoCategory.LoveAuth, "Gathering public key...");
        LoveAuthPubkeyResponse pubkey = this.Get<LoveAuthPubkeyResponse>("/api/publickey");
        
        this.Logger.LogInfo(ResoCategory.LoveAuth, "Successfully got the server's public key!");
        this.Logger.LogTrace(ResoCategory.LoveAuth, $"Algorithm: {pubkey.Algorithm}, Key: {pubkey.Key}");

        KeyHelper.SetKey(pubkey.Key);
    }

    public LoveAuthUserResponse? ValidateJwt(string token)
    {
        string? json = JwtBuilder.Create()
            .WithAlgorithm(new FoxAlgorithm())
            .MustVerifySignature()
            .Decode(token);

        if (json == null)
            return null;

        LoveAuthUserResponse? user = JsonConvert.DeserializeObject<LoveAuthUserResponse>(json);
        if (user == null)
            return null;
        
        // TODO: check user.audience
        if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() < user.Expiration)
            throw new TokenExpiredException("User has expired.");

        return user;
    }
}