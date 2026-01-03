using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using WebCommander.Models;
using WebCommander.Helpers;
using Shared;

namespace WebCommander.Commands
{
    public abstract class ExecuteCommand : NonParsedCommand
    {
        public override string GetUsage()
        {
            return $"Usage: {this.Name} [toolName] [arguments]";
        }

        public override async Task<CommandResult> ExecuteAsync(string cmdLine)
        {
            this.CommandLine = cmdLine;
            CommandResult cmdResult = new CommandResult();
            try
            {
                var parms = new ParameterDictionary();
                var args = cmdLine.GetArgs();
                if(args.Count() < 2)
                {
                    return new CommandResult().Failed($"{this.GetUsage()}");
                }

                string toolName = args[1];
                string actualCmdLine = cmdLine.ExtractAfterParam(1);
                
                parms.AddParameter(ParameterId.Name, toolName);
                if (!string.IsNullOrWhiteSpace(actualCmdLine))
                    parms.AddParameter(ParameterId.Parameters, actualCmdLine);
                parms.AddParameter(ParameterId.Output, true);
                
                var taskId = await this._client.TaskAgent(this.CommandLine, this._agent.Id, Id, parms);
                return cmdResult.Succeed($"Command {this.Name} tasked to agent {this._agent.Id}.", taskId);
            }
            catch (Exception ex)
            {
                return cmdResult.Failed($"[Error] Failed to send task: {ex.Message}");
            }
            return cmdResult;
        }
    }

    
}
