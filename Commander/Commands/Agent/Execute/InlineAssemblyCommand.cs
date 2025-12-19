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

    public class InlineAssemblyAssemblyCommand : SimpleEndPointCommand
    {
        public override string Description => "Execute a dot net assembly in memory";
        public override string Category => CommandCategory.Execution;
        public override string Name => "inline-assembly";
        public override Shared.OsType[] SupportedOs => new[] { Shared.OsType.Windows };
        public override CommandId CommandId => CommandId.Assembly;

        protected override void SpecifyParameters(CommandContext context)
        {
            var agent = context.Executor.CurrentAgent;
            var args = context.CommandParameters.GetArgs();
            var exePath = args[0];

            var binParams = context.CommandParameters.Substring(exePath.Length);

            context.AddParameter(ParameterId.Name, exePath);
            context.AddParameter(ParameterId.Parameters, binParams);
            context.AddParameter(ParameterId.Output, true);
        }

        protected override bool CheckParams(CommandContext context)
        {
            var args = context.CommandParameters.GetArgs();
            if (args.Length == 0)
            {
                context.Terminal.WriteLine($"Usage : {this.Name} AssemblyPathPath [Arguments]");
                return false;
            }

            return base.CheckParams(context);
        }
    }
}
