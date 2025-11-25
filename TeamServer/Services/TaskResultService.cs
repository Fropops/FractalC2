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

    public interface ITaskResultService : IStorable
    {
        void AddTaskResult(AgentTaskResult res);
        IEnumerable<AgentTaskResult> GetAgentTaskResults();
        AgentTaskResult GetAgentTaskResult(string id);
        void Remove(AgentTaskResult result);
    }
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

        public void AddTaskResult(AgentTaskResult res)
        {
            if (!_results.ContainsKey(res.Id))
            {
                _results.Add(res.Id, res);
                this._dbService.Insert((ResultDao)res).Wait();
            }
            else
            {
                var existing = _results[res.Id];
                existing.Status = res.Status;
                existing.Output += res.Output;
                existing.Error = res.Error;
                existing.Info = res.Info;
                existing.Objects = res.Objects;

                this._dbService.Update((ResultDao)res).Wait();
            }
        }

        public void Remove(AgentTaskResult result)
        {
            var dao = (ResultDao)result;
            dao.IsDeleted = true;
            this._dbService.Update(dao).Wait();
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
