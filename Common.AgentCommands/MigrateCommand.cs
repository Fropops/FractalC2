using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;
using Common.Payload;

namespace Common.AgentCommands
{
    public class MigrateCommandOptions : CommandOption
    {
        [Argument("processId", "id of the process to injects to", 0, IsRequired = true)]
        public int ProcessId { get; set; }

        [Option("b", "endpoint", "EndPoint to Bind To")]
        public string Endpoint { get; set; }

        [Option("k", "serverKey", "The server unique key of the endpoint")]
        public string ServerKey { get; set; }

        [Option("x86", "x86", "Generate a x86 architecture executable")]
        public bool X86 { get; set; }

        [Option("v", "verbose", "Show details of the command execution.")]
        public bool Verbose { get; set; }
    }

    [Command("migrate", "Migrate the current agent to another process", Category = AgentCommandCategories.Execution)]
    public class MigrateCommand : AgentCommand<MigrateCommandOptions>
    {
        public override CommandId CommandId => CommandId.Inject;

        public override async Task<bool> Execute(AgentCommandContext context, MigrateCommandOptions options)
        {
            var agentMetadata = context.Metadata;
            if (string.IsNullOrEmpty(options.Endpoint))
            {
                context.WriteInfo($"No Endpoint selected, taking the current agent enpoint ({agentMetadata.EndPoint})");
                options.Endpoint = agentMetadata.EndPoint;
            }

            var endpoint = ConnexionUrl.FromString(options.Endpoint);
            if (!endpoint.IsValid)
            {
                context.WriteError($"[X] EndPoint is not valid !");
                return false;
            }

            context.AddParameter(ParameterId.Id, options.ProcessId);
            context.AddParameter(ParameterId.Bind, endpoint.ToString());
            context.AddParameter(ParameterId.Target, options.X86 ? "x86" : "x64");
            context.TaskAgent(options.CommandLine, this.CommandId, context.GetParameters());

            return true;
        }
    }
}
