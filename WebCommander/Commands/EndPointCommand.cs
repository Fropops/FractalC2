using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Shared;
using WebCommander.Models;
using WebCommander.Services;

namespace WebCommander.Commands
{
    public abstract class EndPointCommand : ParsedCommand
    {
        public override async Task<CommandResult> ExecuteAsync(ParseResult parseResult, TeamServerClient client, Agent agent)
        {
            CommandResult cmdResult = new CommandResult();
            try
            {
                var parms = new ParameterDictionary();
                await FillParametersAsync(parseResult, parms);
            
                var taskId = await client.TaskAgent(this.CommandLine, agent.Id, this.Id, parms);
                return cmdResult.Succeed($"Command {this.Name} tasked to agent {agent.Metadata?.Name}.", taskId);
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
