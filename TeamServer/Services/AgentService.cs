using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.APIModels;
using Shared;
using TeamServer.Database;
using TeamServer.Models;
using TeamServer.Service;

namespace TeamServer.Services
{
    [InjectableService]
    public interface IAgentService : IStorable
    {
        Task AddAgentAsync(Agent agent);
        IEnumerable<Agent> GetAgents();
        Agent GetAgent(string id);
        Task RemoveAgentAsync(Agent agent);
        List<Agent> GetAgentToRelay(string id);
        Task<Agent> GetOrCreateAgentAsync(string agentId);

        Task CheckinAsync(Agent agent, AgentMetadata metaData = null);
    }

    [InjectableServiceImplementation(typeof(IAgentService))]
    public class AgentService : IAgentService
    {
        private readonly IChangeTrackingService _changeTrackingService;
        private readonly IDatabaseService _dbService;
        public AgentService(IChangeTrackingService changeTrackingService, IDatabaseService dbService)
        {
            _changeTrackingService = changeTrackingService;
            _dbService = dbService;
        }

        private readonly ConcurrentDictionary<string, Agent> _agents = new();

        public async Task AddAgentAsync(Agent agent)
        {
            _agents[agent.Id] = agent;

            var existingDbAgent = await this._dbService.Get<AgentDao>(d => d.Id == agent.Id);
            if (existingDbAgent != null)
                await this._dbService.Update((AgentDao)agent);
            else
                await this._dbService.Insert((AgentDao)agent);
        }

        public async Task CheckinAsync(Agent agent, AgentMetadata metaData = null)
        {
            agent.LastSeen = DateTime.UtcNow;
            if (metaData != null)
                agent.Metadata = metaData;
            await this.AddAgentAsync(agent);
        }


        public Agent GetAgent(string id)
        {
            _agents.TryGetValue(id, out var agent);
            return agent;
        }

        public List<Agent> GetAgentToRelay(string id)
        {
            return GetAgents().ToList().Where(a => a.Id == id || a.RelayId == id).ToList();
        }

        public IEnumerable<Agent> GetAgents()
        {
            return _agents.Values.ToList();
        }

        public async Task RemoveAgentAsync(Agent agent)
        {
            _agents.TryRemove(agent.Id, out _);
            AgentDao agentDao = agent;
            agentDao.IsDeleted = true;
            await this._dbService.Update(agentDao);
        }

        public async Task<Agent> GetOrCreateAgentAsync(string agentId)
        {
            var agent = this.GetAgent(agentId);
            if (agent == null)
            {
                agent = new Agent(agentId);
                await this.AddAgentAsync(agent);
                this._changeTrackingService.TrackChange(ChangingElement.Agent, agentId);
            }
            return agent;
        }

        public async Task LoadFromDB()
        {
            this._agents.Clear();
            var agents = await this._dbService.Load<AgentDao>();
            foreach (var agent in agents)
            {
                if (agent.IsDeleted)
                    continue;

                this._agents[agent.Id] = agent;
            }

        }
    }
}
