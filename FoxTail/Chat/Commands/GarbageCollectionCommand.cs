using System.Diagnostics;
using FoxTail.Chat.CommandSupport;
using FoxTail.Chat.Platforms;
using FoxTail.Chat.Platforms.Console;

namespace FoxTail.Chat.Commands;

public class GarbageCollectionCommand : IChatCommand
{
    public string Name => "gc";
    public string HelpText => "Performs a garbage collection on the server";
    public bool RequirePermission => true;
    public async Task InvokeAsync(HeadlessContext context, IChatChannel channel, IChatUser user, ArgContainer args)
    {
        if (!context.UserConfig.Owner.Ids.Contains(user.UserId) && user is not ConsoleChatUser)
        {
            await channel.SendMessageAsync("You need to be the owner of the server to run this. Sorry!");
            return;
        }
        await channel.SendMessageAsync("Collecting... hang tight!");
        Stopwatch sw = Stopwatch.StartNew();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);
        GC.WaitForPendingFinalizers();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);
        GC.WaitForPendingFinalizers();
        sw.Stop();
        await channel.SendMessageAsync($"**FULL** GC took {sw.ElapsedMilliseconds}ms.");
    }
}