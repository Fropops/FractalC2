using Commander.Executor;
using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Commander.Models;
using Commander.Commands.Agent;
using Common;
using Common.Payload;
using Shared;

namespace Commander.Commands.Laucher
{
    public class MigrateCommandOptions
    {
        public string endpoint { get; set; }
        public bool debug { get; set; }

        public bool x86 { get; set; }

        public bool verbose { get; set; }

        public int? processId { get; set; }

        public string serverKey { get; set; }
    }
    public class MigrateCommand : EnhancedCommand<MigrateCommandOptions>
    {
        public override string Category => CommandCategory.Execution;
        public override string Description => "Migrate the current agent to another process";
        public override string Name => "migrate";

        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;
        public override Shared.OsType[] SupportedOs => new[] { Shared.OsType.Windows };

        public override RootCommand Command => new RootCommand(this.Description)
        {
            new Option<int?>(new[] { "--processId", "-pid" }, () => null, "id of the process to injects to"),
            new Option<string>(new[] { "--endpoint", "-b" }, () => null, "EndPoint to Bind To"),
            new Option<string>(new[] { "--serverKey", "-k" }, () => null, "The server unique key of the endpoint"),
            new Option(new[] { "--x86", "-x86" }, "Generate a x86 architecture executable"),
            new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
        };

        protected override async Task<bool> HandleCommand(CommandContext<MigrateCommandOptions> context)
        {
            var agent = context.Executor.CurrentAgent;
            if (string.IsNullOrEmpty(context.Options.endpoint))
            {
                context.Terminal.WriteLine($"No Endpoint selected, taking the current agent enpoint ({agent.Metadata.EndPoint})");
                context.Options.endpoint = agent.Metadata.EndPoint;
            }

            var endpoint = ConnexionUrl.FromString(context.Options.endpoint);
            if (!endpoint.IsValid)
            {
                context.Terminal.WriteError($"[X] EndPoint is not valid !");
                return false;
            }


            if (context.Options.processId.HasValue)
            {
                context.AddParameter(ParameterId.Id, context.Options.processId.Value);
                await context.CommModule.TaskAgent(context.CommandLabel, agent.Id, Shared.CommandId.Inject, context.Parameters);
            }
            else
            {
                await context.CommModule.TaskAgent(context.CommandLabel, agent.Id, Shared.CommandId.ForkAndRun, context.Parameters);
            }

            context.WriteTaskSendToAgent(this);

            return true;
        }
    }

}
