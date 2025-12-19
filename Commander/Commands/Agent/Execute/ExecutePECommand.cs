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

    public class ExecutePECommand : SimpleEndPointCommand
    {
        public override string Description => "Execute a PE assembly with Fork And Run mechanism";
        public override string Category => CommandCategory.Execution;
        public override string Name => "execute-pe";
        public override Shared.OsType[] SupportedOs => new[] { Shared.OsType.Windows };
        public override CommandId CommandId => CommandId.ForkAndRun;

        protected override void InnerExecute(CommandContext context)
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

            var prms = context.CommandParameters.ExtractAfterParam(0);

            if (string.IsNullOrEmpty(prms))
                context.Terminal.WriteLine($"Generating payload without params...");
            else
                context.Terminal.WriteLine($"Generating payload with params {prms}...");

            context.AddParameter(ParameterId.Name, Path.GetFileName(exePath));
            context.AddParameter(ParameterId.Output, true);

            base.CallEndPointCommand(context);

        }
    }
}
