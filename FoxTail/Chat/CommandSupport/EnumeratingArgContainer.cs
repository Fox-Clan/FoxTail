using System.Collections;

namespace FoxTail.Chat.CommandSupport;

public class EnumeratingArgContainer : ArgContainer
{
    private readonly string[] _args;
    private readonly IEnumerator _enumerator;
    private readonly Dictionary<string, string> _cache = [];

    public EnumeratingArgContainer(string[] args)
    {
        this._args = args;
        this._enumerator = args.GetEnumerator();
    }
    
    public override string? GetArg(string name)
    {
        // if commands ever look up a value twice
        if (this._cache.TryGetValue(name, out string? cachedArg))
        {
            return cachedArg;
        }
        
        if (!this._enumerator.MoveNext())
            return null;

        string? arg = (string?)this._enumerator.Current;
        if (arg == null)
            return null;

        _cache[name] = arg;
        return arg;
    }

    public override string GetAllArgs()
    {
        return string.Join(' ', this._args);
    }
}