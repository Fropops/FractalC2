using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Command
{
    public class CommandExecutionContext<T>
    {
        public ICommandCommander Commander { get; set; }
        public ICommandAgent Agent { get; set; }
        public T Options { get; set; }

        public CommandExecutionContext(ICommandAgent agent, ICommandCommander commander, T options)
        {
            this.Agent = agent;
            this.Commander = commander;
            this.Options = options;
        }
    }
}
