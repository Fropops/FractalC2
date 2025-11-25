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
    public class RunCommand : SimpleEndPointCommand
    {
        public override string Name => "run";
        public override string Description => "Run an executable, capturing output";
        public override CommandId CommandId => CommandId.Run;

        protected override void SpecifyParameters(CommandContext context)
        {
            context.AddParameter(ParameterId.Command, context.CommandParameters.BinarySerializeAsync().Result);
        }
    }

}
