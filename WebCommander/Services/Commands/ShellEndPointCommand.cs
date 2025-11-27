using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using WebCommander.Models;

namespace WebCommander.Services.Commands
{
    public abstract class ShellEndPointCommand : CommandBase
    {
        public abstract CommandId Id { get; }

        override public async Task<CommandResult> ExecuteAsync(ParseResult parseResult, TeamServerClient client, string agentId)
        {
            return new CommandResult();
        }

        public async Task<CommandResult> ExecuteAsync(string cmdName,string cmdLine, TeamServerClient client, string agentId)
        {
            this.CommandLine = cmdLine;
            CommandResult cmdResult = new CommandResult();
            try
            {
                var parms = new ParameterDictionary();
                var command = cmdLine.Remove(0, cmdName.Length).TrimStart();
        
                if (!string.IsNullOrWhiteSpace(command))
                    parms.AddParameter(ParameterId.Command, command);
                
                var taskId = await client.TaskAgent(Name, agentId, Id, parms);
                return cmdResult.Succeed($"Command {this.Name} tasked to agent {agentId}.", taskId);
            }
            catch (Exception ex)
            {
                return cmdResult.Failed($"[Error] Failed to send task: {ex.Message}");
            }
            return cmdResult;
        }
    }
}
