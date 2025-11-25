using System.Threading.Tasks;
using Shared;
using System.CommandLine;
using Commander.Helper;

namespace Commander.Commands.Agent.EndPoint
{
    public class LinkCommandOptions : VerbAwareCommandOptions
    {
        public string bindto { get; set; }
    }
    public class LinkCommand : VerbAwareCommand<LinkCommandOptions>
    {
        public override string Category => CommandCategory.Core;
        public override string Description => "Link to another Agent";
        public override string Name => "link";

        public override CommandId CommandId => CommandId.Link;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                 new Argument<string>("verb", () =>CommandVerbs.Show.Command()).FromAmong(CommandVerbs.Show.Command(), CommandVerbs.Start.Command(), CommandVerbs.Stop.Command()),
                 new Option<string>(new[] { "--bindto", "-b" }, () => null, "Endpoint To bind to"),
            };

        protected override void RegisterVerbs()
        {
            base.RegisterVerbs();
            this.Register(CommandVerbs.Start, this.Start);
            this.Register(CommandVerbs.Stop, this.Stop);
            //this.Register(ServiceVerb.Show, this.Show);
        }

        protected async Task<bool> Start(CommandContext<LinkCommandOptions> context)
        {
            var url = context.Options.bindto;

            ConnexionUrl conn = ConnexionUrl.FromString(url);
            if (conn == null || !conn.IsValid)
            {
                context.Terminal.WriteError($"BindTo is not valid!");
                return false;
            }

            context.AddParameter(ParameterId.Bind, conn.ToString());

            return true;
        }

        protected async Task<bool> Stop(CommandContext<LinkCommandOptions> context)
        {
            var url = context.Options.bindto;

            ConnexionUrl conn = ConnexionUrl.FromString(url);
            if (conn == null || !conn.IsValid)
            {
                context.Terminal.WriteError($"BindTo is not valid!");
                return false;
            }

            context.AddParameter(ParameterId.Bind, conn.ToString());

            return true;
        }

    }
}
