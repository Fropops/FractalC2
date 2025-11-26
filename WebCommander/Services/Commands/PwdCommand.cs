using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using WebCommander.Models;

namespace WebCommander.Services.Commands
{
    public class PwdCommand : EndPointCommand
    {
        public override string Name => "pwd";
        public override string Description => "Print working directory";
        public override CommandId Id => CommandId.Pwd;

        public override Command CreateCommand()
        {
            return new Command(Name, Description);
        }

        public override async Task<(string message, string? taskId)> ExecuteAsync(ParseResult result, TeamServerClient client, string agentId)
        {
            var parms = new ParameterDictionary();

            try
            {
                var taskId = await client.TaskAgent(Name, agentId, Id, parms);
                return ($"{Name} task sent.", taskId);
            }
            catch (Exception ex)
            {
                return ($"[Error] Failed to send task: {ex.Message}", null);
            }
        }
    }
}
