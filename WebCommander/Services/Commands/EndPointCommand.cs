using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using WebCommander.Models;

namespace WebCommander.Services.Commands
{
    public abstract class EndPointCommand : ParsedCommand
    {
        public override async Task<CommandResult> ExecuteAsync(ParseResult parseResult, TeamServerClient client, string agentId)
        {
            CommandResult cmdResult = new CommandResult();
            try
            {
                var parms = new ParameterDictionary();
                await FillParametersAsync(parseResult, parms);
            
                var taskId = await client.TaskAgent(this.CommandLine, agentId, this.Id, parms);
                return cmdResult.Succeed($"Command {this.Name} tasked to agent {agentId}.", taskId);
            }
            catch (Exception ex)
            {
                return cmdResult.Failed($"[Error] Failed to send task: {ex.Message}");
            }
            return cmdResult;
        }   

        public abstract Task FillParametersAsync(ParseResult parseResult, ParameterDictionary parms);       
    }
}
