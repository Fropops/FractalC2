using System;

namespace TeamServer.UI.Models
{
    public class TeamServerAgentTask
    {
        public string Id { get; set; }
        public string AgentId { get; set; }
        public CommandId CommandId { get; set; }
        public string Command { get; set; }
        public DateTime RequestDate { get; set; }

        public TeamServerAgentTask()
        {
        }

        public TeamServerAgentTask(string id, CommandId commandId, string agentId, string command, DateTime requestDate)
        {
            this.Id = id;
            this.AgentId = agentId;
            this.CommandId = commandId;
            this.Command = command;
            this.RequestDate = requestDate;
        }
    }
}
