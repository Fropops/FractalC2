using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Executor;
using Shared;
using System.Security.Cryptography;
using BinarySerializer;
using System.CommandLine;

namespace Commander.Commands.Agent.EndPoint
{
    public class StartCommand : SimpleEndPointCommand
    {
        public override string Name => "start";
        public override string Description => "Start an executable, without capturing output";
        public override CommandId CommandId => CommandId.Start;

        protected override void SpecifyParameters(CommandContext context)
        {
            context.AddParameter(ParameterId.Command, context.CommandParameters.BinarySerializeAsync().Result);
        }
    }

    public class StartAsCommand : SimpleEndPointCommand
    {
        public override string Name => "startas";
        public override string Description => "Start an executable, without capturing output, as another user";
        public override CommandId CommandId => CommandId.StartAs;

        protected override bool CheckParams(CommandContext context)
        {
            var args = context.CommandParameters.GetArgs();
            if (args.Length < 3 || !args[0].Contains('\\'))
            {
                context.Terminal.WriteError($"Usage : {this.Name} domain\\user password executable [args]");
                return false;
            }
            return base.CheckParams(context);
        }

        protected override void SpecifyParameters(CommandContext context)
        {
            var args = context.CommandParameters.GetArgs();

            var split = args[0].Split('\\');
            var domain = split[0];
            var username = split[1];
            var password = args[1];
            context.AddParameter(ParameterId.User, username);
            context.AddParameter(ParameterId.Domain, domain);
            context.AddParameter(ParameterId.Password, password);
            var cmd = context.CommandParameters.ExtractAfterParam(1);
            context.AddParameter(ParameterId.Command, cmd);
        }
    }

}
