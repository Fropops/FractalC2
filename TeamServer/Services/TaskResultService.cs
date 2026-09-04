using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Models;
using Shared;
using TeamServer.Database;
using TeamServer.Models;
using TeamServer.Service;


namespace TeamServer.Services
{
    [InjectableService]
    public interface ITaskResultService : IStorable
    {
        Task AddTaskResultAsync(AgentTaskResult res);
        IEnumerable<AgentTaskResult> GetAgentTaskResults();
        AgentTaskResult GetAgentTaskResult(string id);
        Task RemoveAsync(AgentTaskResult result);
    }
    [InjectableServiceImplementation(typeof(ITaskResultService))]
    public class TaskResultService : ITaskResultService
    {
        private readonly IDatabaseService _dbService;
        public TaskResultService(IDatabaseService dbService)
        {
            this._dbService = dbService;
        }

        private readonly Dictionary<string, AgentTaskResult> _results = new();

        public async Task LoadFromDB()
        {
            this._results.Clear();
            var results = await this._dbService.Load<ResultDao>();
            foreach (var result in results)
            {
                if(result.IsDeleted) continue;
                this._results.Add(result.Id, result);
            }

        }

        public async Task AddTaskResultAsync(AgentTaskResult res)
        {
            if (!_results.ContainsKey(res.Id))
            {
                _results.Add(res.Id, res);
                await this._dbService.Insert((ResultDao)res);
            }
            else
            {
                var existing = _results[res.Id];
                existing.Status = res.Status;
                existing.Output += res.Output;
                existing.Error = res.Error;
                existing.Info = res.Info;
                existing.Objects = res.Objects;

                await this._dbService.Update((ResultDao)res);
            }
        }

        public async Task RemoveAsync(AgentTaskResult result)
        {
            var dao = (ResultDao)result;
            dao.IsDeleted = true;
            await this._dbService.Update(dao);
            _results.Remove(result.Id);
        }

        public AgentTaskResult GetAgentTaskResult(string id)
        {
            if (!_results.ContainsKey(id))
                return null;
            return _results[id];
        }

        public IEnumerable<AgentTaskResult> GetAgentTaskResults()
        {
            return _results.Values;
        }

    }
}
