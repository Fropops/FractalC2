using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using TeamServer.UI.Models;

namespace TeamServer.UI.Services.Commands
{
    public class CdCommand : EndPointCommand
    {
        public override string Name => "cd";
        public override string Description => "Change directory";
        public override CommandId Id => CommandId.Cd;

        private Argument<string> _pathArg;

        public override Command CreateCommand()
        {
            var command = new Command(Name, Description);
            _pathArg = new Argument<string>("path") { Description = "Path to change to" };
            command.Add(_pathArg);
            return command;
        }

        public override async Task<(string message, string? taskId)> ExecuteAsync(ParseResult result, TeamServerClient client, string agentId)
        {
            var path = result.GetValue(_pathArg);
            var parms = new ParameterDictionary();
            if (!string.IsNullOrEmpty(path))
            {
                parms.AddParameter(ParameterId.Path, path);
            }

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
