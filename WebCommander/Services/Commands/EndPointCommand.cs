using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using TeamServer.UI.Models;

namespace TeamServer.UI.Services.Commands
{
    public abstract class EndPointCommand : CommandBase
    {
        public abstract CommandId Id { get; }

        public abstract Task<(string message, string? taskId)> ExecuteAsync(ParseResult result, TeamServerClient client, string agentId);
    }
}
