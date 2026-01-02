using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Executor;
using Common.Command;
using Common.Command.Custom;
using Shared;

namespace Commander.Commands.Custom
{
    public abstract class CommanderCustomCommand<Comm, Opt> : EnhancedCommand<Opt> where Comm : Common.Command.CustomCommand<Opt>, new() where Opt : class, new()
    {
        public override string Description => this.CustomCommand.Description;
        public override string Name => this.CustomCommand.Name;

        public override Shared.OsType[] SupportedOs => this.CustomCommand.SupportedOs;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override string Category => CommandCategory.Custom;

        public override string[] Alternate => this.CustomCommand.Alternate;

        public override RootCommand Command =>
            CommandGenerator.GenerateRootCommand<Opt>(this.Description);

        private Comm CustomCommand { get; set; }
        public CommanderCustomCommand()
        {
            this.CustomCommand = new Comm();
        }

        protected override async Task<bool> HandleCommand(CommandContext<Opt> context)
        {
            ICommandAgent agent = new CustomCommandAgentAdaptater<Opt>(context, new List<AgentTask>());
            ICommandCommander commander = new CustomCommandCommanderAdaptater<Opt>(context);
            await this.CustomCommand.Execute(new CommandExecutionContext<Opt>(agent, commander, context.Options));

            return true;
        }
    }
}
