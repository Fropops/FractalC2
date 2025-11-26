using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using WebCommander.Models;

namespace WebCommander.Services.Commands
{
    public class LsCommand : EndPointCommand
    {
        public override string Name => "ls";
        public override string Description => "List directory contents";
        public override CommandId Id => CommandId.Ls;
        public override string[] Aliases => new[] { "dir" };

        private Option<string> _pathArg;

        public override Command CreateCommand()
        {
            var command = new Command(Name, Description);
            // command.AddAlias("dir"); // AddAlias issue in beta
            _pathArg = new Option<string>("path") { Description = "Path to list" };
            // Default value handled manually in service
            command.Add(_pathArg);
            return command;
        }

        public override async Task<(string message, string? taskId)> ExecuteAsync(ParseResult result, TeamServerClient client, string agentId)
        {
            var parms = new ParameterDictionary();
            var path = result.GetValue(_pathArg);
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
