using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using WebCommander.Models;
using WebCommander.Helpers;
using Shared;

namespace WebCommander.Commands
{
    public abstract class NonParsedCommand : CommandBase
    {
        public override string GetUsage()
        {
            return $"Usage: {this.Name} [command]";
        }

        public override async Task<CommandResult> ExecuteAsync(string cmdLine)
        {
            this.CommandLine = cmdLine;
            CommandResult cmdResult = new CommandResult();
            try
            {
                var parms = new ParameterDictionary();
                var args = cmdLine.GetArgs();
                string cmdName = args[0];
                string actualCmdLine = cmdLine.ExtractAfterParam(0);
                if(string.IsNullOrWhiteSpace(actualCmdLine))
                {
                    return new CommandResult().Failed($"{this.GetUsage()}");
                }
        
                if (!string.IsNullOrWhiteSpace(actualCmdLine))
                    parms.AddParameter(ParameterId.Command, actualCmdLine);
                
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
