using FoxTail.Chat.Platforms;
using FoxTail.Worlds;
using SkyFrost.Base;

namespace FoxTail.Chat.CommandSupport;

public abstract class ArgContainer
{
    public abstract string? GetArg(string name);
    public abstract string GetAllArgs();

    public Uri? GetUri(string name, HeadlessContext? context = null)
    {
        string? uriStr = GetArg(name);
        if (uriStr == null)
            return null;

        string? knownUriStr = context?.WorldConfig.GetKnownWorldUrlById(uriStr);
        if (knownUriStr != null)
            uriStr = knownUriStr;

        Uri.TryCreate(uriStr, UriKind.Absolute, out Uri? uri);
        return uri;
    }

    public Uri? GetRecordUrl(string name, HeadlessContext? context = null)
    {
        Uri? uri = GetUri(name, context);
        if (uri == null)
            return null;
        
        if (uri.Scheme.StartsWith('h'))
            uri = new Uri(uri.ToString().Replace("https://go.resonite.com/record", "resrec://"));

        if (uri.Scheme != "resrec")
            return null;
        
        return uri;
    }

    public uint? GetUint(string name)
    {
        string? uintStr = GetArg(name);
        if (uintStr == null)
            return null;

        bool success = uint.TryParse(uintStr, out uint result);
        if (!success)
            return null;

        return result;
    }

    public ManagedWorld? WorldByIdOrUserFocused(HeadlessContext context, IChatUser user)
    {
        uint? worldId = GetUint("worldId");
        if (worldId == null)
            return context.WorldManager.FindWorldUserIn(user);

        return context.WorldManager.FindWorldById(worldId.Value);
    }

    public async Task<User?> GetCloudUserAsync(string name, HeadlessContext context)
    {
        string? username = GetArg(name);
        if (username == null)
            return null;

        CloudResult<User>? user = await context.Engine.Cloud.Users.GetUserByName(username);
        if (user == null)
            return null;

        return user.Entity;
    }
}