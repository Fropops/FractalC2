using System.Threading.Tasks;
using Shared;
using System.CommandLine;
using Commander.Helper;

namespace Commander.Commands.Agent.EndPoint
{
    public class KeyLogCommandOptions : VerbAwareCommandOptions
    {
    }
    public class KeyLogCommand : VerbAwareCommand<KeyLogCommandOptions>
    {
        public override string Category => CommandCategory.Core;
        public override string Description => "Log keys on the agent";
        public override string Name => "keylog";

        public override CommandId CommandId => CommandId.KeyLog;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("verb", () =>CommandVerbs.Show.Command()).FromAmong(CommandVerbs.Show.Command(), CommandVerbs.Start.Command(), CommandVerbs.Stop.Command()),
            };


       

    }
}
