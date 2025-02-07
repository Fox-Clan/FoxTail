using System.Collections.Frozen;
using System.Reflection;
using FrooxEngine;
using NotEnoughLogs;

namespace JvyHeadlessRunner.EngineIntegration.LoadManagement;

public class InitTaskManager
{
    private readonly Logger _logger;
    private readonly Engine _engine;
    private readonly IEngineInitProgress _progress;

    private readonly FrozenSet<InitTask> _tasks; 
    
    public InitTaskManager(Logger logger, Engine engine, IEngineInitProgress progress)
    {
        this._logger = logger;
        this._engine = engine;
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
                await this._engine.GlobalCoroutineManager.StartTask(async () =>
                {
                    this._progress.SetSubphase(tasks.Key.ToString() + '/' + task.Name);
                    await new NextUpdate();
                    await task.ExecuteAsync(this._logger, this._engine);
                });
            }
        }
    }
}