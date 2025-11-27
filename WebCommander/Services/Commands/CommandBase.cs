using System.CommandLine;
using WebCommander.Models;

namespace WebCommander.Services.Commands
{
    public abstract class CommandBase
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public virtual string[] Aliases { get; } = Array.Empty<string>();

        public RootCommand Command { get; private set;}

        public CommandResult Result { get; protected set;}

        public abstract Task<CommandResult> ExecuteAsync(ParseResult result, TeamServerClient client, string agentId);

        public CommandBase()
        {
        }

        public void Initialize(TeamServerClient client, string agentId = null)
        {
            this.CreateCommand(client, agentId);
        }

        public ParseResult Parse(string input)
        {
            return this.Command.Parse(input);
        }

        protected virtual RootCommand CreateCommand(TeamServerClient client, string agentId) 
        {
            this.Command = new RootCommand(Description);
            this.Command.Add(new Argument<string>(this.Name)
            {
                Arity = ArgumentArity.ExactlyOne,
            });

            this.Command.SetAction(async (parseResult, cancellationToken) =>
            {
              this.Result = await this.ExecuteAsync(parseResult, client, agentId);
            });

            this.AddCommandParameters(this.Command);
            return this.Command;
        }

        protected virtual void AddCommandParameters(RootCommand command)
        {
        } 

        public string GetUsage()
        {
            var parts = new List<string>();
            Command cmd = this.Command;

            // Nom de la commande
            parts.Add(cmd.Arguments.First().Name);

            // Arguments
            foreach (var arg in cmd.Arguments.Skip(1))
            {
                string syntax = arg.Arity.MinimumNumberOfValues > 0
                    ? $"<{arg.Name}>"
                    : $"[{arg.Name}]";

                parts.Add(syntax);
            }

            // Options
            foreach (var opt in cmd.Options)
            {
                if (opt.Name == "--version")
                    continue;
                string syntax = opt.Arity.MinimumNumberOfValues > 0
                    ? $"<{opt.Name}>"
                    : $"[{opt.Name}]";

                parts.Add(syntax);
            }

            return "Usage: " + string.Join(" ", parts);
        }
    }
}
