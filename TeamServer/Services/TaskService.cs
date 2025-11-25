using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Models;
using Shared;
using TeamServer.Database;

namespace TeamServer.Service;

public interface ITaskService : IStorable
{
    void Add(TeamServerAgentTask task);

    TeamServerAgentTask Get(string id);

    List<TeamServerAgentTask> RemoveAgent(string agentId);

    List<TeamServerAgentTask> GetForAgent(string agentId);
}

public class TaskService : ITaskService
{
    private readonly IDatabaseService _dbService;

    private Dictionary<string, TeamServerAgentTask> _tasks = new Dictionary<string, TeamServerAgentTask>();
    private Dictionary<string, List<TeamServerAgentTask>> _agentTasks = new Dictionary<string, List<TeamServerAgentTask>>();

    public TaskService(IDatabaseService dbService)
    {
        _dbService = dbService;
    }

    public void Add(TeamServerAgentTask task)
    {
        _tasks.Add(task.Id, task);
        if (!_agentTasks.ContainsKey(task.AgentId))
            _agentTasks.Add(task.AgentId, new List<TeamServerAgentTask>() { task });
        else
            _agentTasks[task.AgentId].Add(task);

        this._dbService.Insert((TaskDao)task).Wait();
    }

    public TeamServerAgentTask Get(string id)
    {
        if (!this._tasks.ContainsKey(id))
            return null;

        return this._tasks[id];
    }

    public List<TeamServerAgentTask> GetForAgent(string agentId)
    {
        if(!_agentTasks.ContainsKey(agentId))
            return new List<TeamServerAgentTask>();

        return _agentTasks[agentId];
    }

    public List<TeamServerAgentTask> RemoveAgent(string agentId)
    {
        if(!_agentTasks.ContainsKey(agentId))
            return new List<TeamServerAgentTask>();
        var tasks = _agentTasks[agentId];
        _agentTasks.Remove(agentId);
        foreach(var task in tasks)
        {
            var dao = (TaskDao)task;
            dao.IsDeleted = true;
            this._dbService.Update(dao).Wait();
            this._tasks.Remove(task.Id);
        }
        return tasks;
    }

    public async Task LoadFromDB()
    {
        this.Clear();
        var tasks = await _dbService.Load<TaskDao>();
        foreach(var task in tasks)
        {
            if(task.IsDeleted) continue;

            _tasks.Add(task.Id, task);
            if (!_agentTasks.ContainsKey(task.AgentId))
                _agentTasks.Add(task.AgentId, new List<TeamServerAgentTask>() { task });
            else
                _agentTasks[task.AgentId].Add(task);
        }
    }

    public void Clear()
    {
        this._tasks.Clear();
        this._agentTasks.Clear();
    }
}