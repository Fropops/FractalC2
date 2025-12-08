using System.CommandLine;
using WebCommander.Models;

namespace WebCommander.Services.Commands
{
    public abstract class ParsedCommand : CommandBase
    {
        public RootCommand Command { get; private set;}

        public abstract Task<CommandResult> ExecuteAsync(ParseResult result, TeamServerClient client, Agent agent);

        public ParseResult Parse(string input)
        {
            this.CommandLine = input;
            return this.Command.Parse(input);
        }

        public override void Initialize(TeamServerClient client, Agent agent = null, CommandService commandService = null)
        {
            base.Initialize(client, agent, commandService);
            this.CreateCommand(client, agent);
        }

        protected virtual RootCommand CreateCommand(TeamServerClient client, Agent agent) 
        {
            this.Command = new RootCommand(Description);
            this.Command.Add(new Argument<string>(this.Name)
            {
                Arity = ArgumentArity.ExactlyOne,
            });

            this.Command.SetAction(async (parseResult, cancellationToken) =>
            {
              this.Result = await this.ExecuteAsync(parseResult, client, agent);
            });

            this.AddCommandParameters(this.Command);
            return this.Command;
        }

        public override async Task<CommandResult> ExecuteAsync(string cmdLine)
        {
            var parseResult = this.Parse(cmdLine);
            // Handle Help or Errors
            if (parseResult.Errors.Count > 0)
            {
                //var cmd = parseResult.CommandResult.Command;
                //var errorMsg = parseResult.Errors.Count > 0 ? $"{string.Join(", ", parseResult.Errors.Select(e => e.Message))}\n" : "";
                return new CommandResult().Failed($"Error in Arguments\n{this.GetUsage()}");
            }

            if(parseResult.Tokens.Any(t => t.Value == "-h" || t.Value == "--help" || t.Value == "/?"))
            {
                return new CommandResult().Succeed($"{this.GetUsage()}");
            }

            await parseResult.InvokeAsync();
            return this.Result;
        }


        protected virtual void AddCommandParameters(RootCommand command)
        {
        } 

        public override string GetUsage()
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
                if (opt.Name == "--version" || opt.Name == "--help")
                    continue;
                string aliases = string.Join(',', opt.Aliases);
                string syntax = opt.Arity.MinimumNumberOfValues > 0
                    ? $"<{opt.Name} ({aliases})>"
                    : $"[{opt.Name} ({aliases})]";
                
                parts.Add(syntax);
            }

            return "Usage: " + string.Join(" ", parts);
        }
    }
}
