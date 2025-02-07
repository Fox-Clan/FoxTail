using System.Collections.Frozen;
using System.Reflection;
using FrooxEngine;
using NotEnoughLogs;

namespace JvyHeadlessRunner.EngineIntegration.LoadManagement;

public class InitTaskManager
{
    private readonly HeadlessContext _context;
    private readonly IEngineInitProgress _progress;

    private readonly FrozenSet<InitTask> _tasks; 
    
    public InitTaskManager(HeadlessContext context, IEngineInitProgress progress)
    {
        this._context = context;
        this._progress = progress;

        this._tasks = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(c => c.IsAssignableTo(typeof(InitTask)) && c != typeof(InitTask))
            .Select(t => (InitTask?)Activator.CreateInstance(t))
            .ToFrozenSet()!;
    }

    public async Task DoAllTasksAsync()
    {
        IOrderedEnumerable<IGrouping<InitTaskStage, InitTask>> taskGroup = this._tasks
            .GroupBy(t => t.Stage)
            .OrderBy(g => g.Key);

        foreach (IGrouping<InitTaskStage,InitTask> tasks in taskGroup)
        {
            this._progress.SetSubphase(tasks.Key.ToString());
            foreach (InitTask task in tasks)
            {
                await this._context.Engine.GlobalCoroutineManager.StartTask(async () =>
                {
                    this._progress.SetSubphase(tasks.Key.ToString() + '/' + task.Name);
                    await new NextUpdate();
                    await task.ExecuteAsync(this._context);
                });
            }
        }
    }
}