using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using Common.CommandLine.Core;

namespace Commander.Commands
{
    public class CommanderCommandContext : CommandContext
    {
        public ICommModule CommModule { get; private set; }
        public ITerminal Terminal { get; private set; }

        public IExecutor Executor { get; private set; }
        public CommanderCommandContext(ICommModule commModule, ITerminal terminal, IExecutor executor)
        {
            this.CommModule = commModule;
            this.Terminal = terminal;
            this.Executor = executor;
        }

        public bool? IsAgentAlive(Models.Agent agent)
        {
            if (agent.Metadata == null)
                return null;

            if (agent.Metadata.SleepInterval == 0)
            {
                if (agent.LastSeen.AddSeconds(3) >= DateTime.UtcNow)
                    return true;
                return false;
            }

            int delta = 0;
            if (!string.IsNullOrEmpty(agent.RelayId))
            {
                var relay = this.CommModule.GetAgent(agent.RelayId);
                if (relay == null)
                    return null;
                delta = Math.Min(3, relay.Metadata.SleepInterval) * 3;
            }
            else
                delta = Math.Min(3, agent.Metadata.SleepInterval) * 3;

            if (agent.LastSeen.AddSeconds(delta) >= DateTime.UtcNow)
                return true;

            return false;
        }
    }
}
