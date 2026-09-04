using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Models;
using Shared;
using TeamServer.Database;
using TeamServer.Services;

namespace TeamServer.Service;

[InjectableService]
public interface ITaskService : IStorable
{
    Task AddAsync(TeamServerAgentTask task);

    TeamServerAgentTask Get(string id);

    Task<List<TeamServerAgentTask>> RemoveAgentAsync(string agentId);

    List<TeamServerAgentTask> GetForAgent(string agentId);
}

[InjectableServiceImplementation(typeof(ITaskService))]
public class TaskService : ITaskService
{
    private readonly IDatabaseService _dbService;

    private readonly ConcurrentDictionary<string, TeamServerAgentTask> _tasks = new();
    private readonly ConcurrentDictionary<string, List<TeamServerAgentTask>> _agentTasks = new();

    public TaskService(IDatabaseService dbService)
    {
        _dbService = dbService;
    }

    public async Task AddAsync(TeamServerAgentTask task)
    {
        _tasks[task.Id] = task;
        var taskList = _agentTasks.GetOrAdd(task.AgentId, _ => new List<TeamServerAgentTask>());
        lock (taskList)
        {
            taskList.Add(task);
        }

        await this._dbService.Insert((TaskDao)task);
    }

    public TeamServerAgentTask Get(string id)
    {
        return _tasks.TryGetValue(id, out var task) ? task : null;
    }

    public List<TeamServerAgentTask> GetForAgent(string agentId)
    {
        if (!_agentTasks.TryGetValue(agentId, out var taskList))
            return new List<TeamServerAgentTask>();

        lock (taskList)
        {
            return new List<TeamServerAgentTask>(taskList);
        }
    }

    public async Task<List<TeamServerAgentTask>> RemoveAgentAsync(string agentId)
    {
        if (!_agentTasks.TryRemove(agentId, out var tasks))
            return new List<TeamServerAgentTask>();

        List<TeamServerAgentTask> tasksCopy;
        lock (tasks)
        {
            tasksCopy = new List<TeamServerAgentTask>(tasks);
        }

        foreach (var task in tasksCopy)
        {
            var dao = (TaskDao)task;
            dao.IsDeleted = true;
            await this._dbService.Update(dao);
            this._tasks.TryRemove(task.Id, out _);
        }
        return tasksCopy;
    }

    public async Task LoadFromDB()
    {
        this.Clear();
        var tasks = await _dbService.Load<TaskDao>();
        foreach (var task in tasks)
        {
            if (task.IsDeleted) continue;

            _tasks[task.Id] = task;
            var taskList = _agentTasks.GetOrAdd(task.AgentId, _ => new List<TeamServerAgentTask>());
            lock (taskList)
            {
                taskList.Add(task);
            }
        }
    }

    public void Clear()
    {
        this._tasks.Clear();
        this._agentTasks.Clear();
    }
}