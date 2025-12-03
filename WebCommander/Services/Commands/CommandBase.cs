using System.CommandLine;
using WebCommander.Models;

namespace WebCommander.Services.Commands
{
    public abstract class CommandBase
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract CommandId Id { get; }
        public virtual string Category { get; } = CommandCategory.Core;
        public virtual string[] Aliases { get; } = Array.Empty<string>();

        public string CommandLine { get; protected set;}

        public CommandResult Result { get; protected set;}

        public CommandBase()
        {
        }

        protected TeamServerClient _client {get; set;}
        protected Agent _agent {get; set;}
        protected CommandService _commandService {get;set;}

        public virtual void Initialize(TeamServerClient client, Agent agent = null, CommandService commandService = null)
        {
            this._client = client;
            this._agent = agent;
            this._commandService = commandService;
        }

        public virtual async Task<CommandResult> ExecuteAsync(string commandLine)
        {
            this.CommandLine = commandLine;
            return new CommandResult().Failed("Command not implemented");
        }

        public abstract string GetUsage();
    }
}
