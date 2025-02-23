namespace FoxTail.Chat.CommandSupport;

public abstract class ArgContainer
{
    public abstract string? GetArg(string name);
    public abstract string GetAllArgs();
}