using System;
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

    public interface IAgentService : IStorable
    {
        void AddAgent(Agent agent);
        IEnumerable<Agent> GetAgents();
        Agent GetAgent(string id);
        void RemoveAgent(Agent agent);
        List<Agent> GetAgentToRelay(string id);
        Agent GetOrCreateAgent(string agentId);

        void Checkin(Agent agent, AgentMetadata metaData = null);
    }
    public class AgentService : IAgentService
    {
        private readonly IChangeTrackingService _changeTrackingService;
        private readonly IDatabaseService _dbService;
        public AgentService(IChangeTrackingService changeTrackingService, IDatabaseService dbService)
        {
            _changeTrackingService = changeTrackingService;
            _dbService = dbService;
        }

        private readonly Dictionary<string, Agent> _agents = new();

        public void AddAgent(Agent agent)
        {
            if (!_agents.ContainsKey(agent.Id))
                _agents.Add(agent.Id, agent);
            else
                _agents[agent.Id] = agent;

            var existingDbAgent = this._dbService.Get<AgentDao>(d => d.Id == agent.Id).Result;
            if (existingDbAgent != null)
                this._dbService.Update((AgentDao)agent).Wait();
            else
                this._dbService.Insert((AgentDao)agent).Wait();
        }

        public void Checkin(Agent agent, AgentMetadata metaData = null)
        {
            agent.LastSeen = DateTime.UtcNow;
            if (metaData != null)
                agent.Metadata = metaData;
            this.AddAgent(agent);
        }


        public Agent GetAgent(string id)
        {
            if (!_agents.ContainsKey(id))
                return null;
            return _agents[id];
        }

        public List<Agent> GetAgentToRelay(string id)
        {
            return GetAgents().ToList().Where(a => a.Id == id || a.RelayId == id).ToList();
        }

        public IEnumerable<Agent> GetAgents()
        {
            return _agents.Values;
        }

        public void RemoveAgent(Agent agent)
        {
            _agents.Remove(agent.Id);
            AgentDao agentDao = agent;
            agentDao.IsDeleted = true;
            this._dbService.Update(agentDao).Wait();
        }

        public Agent GetOrCreateAgent(string agentId)
        {
            var agent = this.GetAgent(agentId);
            if (agent == null)
            {
                agent = new Agent(agentId);
                this.AddAgent(agent);
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

                this._agents.Add(agent.Id, agent);
            }

        }
    }
}
