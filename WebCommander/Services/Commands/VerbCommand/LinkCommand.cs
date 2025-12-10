using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using WebCommander.Models;

namespace WebCommander.Services.Commands
{
    public class LinkCommand : VerbAwareCommand
    {
        public override string Name => "link";
        public override string Description => "Manage agent links";
        public override CommandId Id => CommandId.Link;
        override protected List<CommandVerbs> AllowedVerbs => new List<CommandVerbs> { CommandVerbs.Show, CommandVerbs.Start, CommandVerbs.Stop };
        private string bindParam = "--endpoint";

        protected override void AddCommandParameters(RootCommand command)
        {
            base.AddCommandParameters(command);
            command.Options.Add(new Option<string>(bindParam, "-b") { Arity = ArgumentArity.ZeroOrOne, Description = "Endpoint to bind to" });
        }

        public override async Task FillParametersAsync(ParseResult parseResult, ParameterDictionary parms)
        {
            await base.FillParametersAsync(parseResult, parms);
            var bind = parseResult.GetValue<string>(bindParam);

            if (!string.IsNullOrEmpty(bind))
            {
                var connexionUrl = ConnexionUrl.FromString(bind);
                if(!connexionUrl.IsValid)
                    throw new ArgumentException("Endpoint is not valid");

                if(connexionUrl.Mode != ConnexionMode.Client)
                    throw new ArgumentException("Endpoint is client mode");

                parms.AddParameter(ParameterId.Bind, bind);
            }
        }
    }
}
