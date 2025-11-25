using System.Threading.Tasks;
using Shared;
using System.CommandLine;
using Commander.Helper;

namespace Commander.Commands.Agent.EndPoint
{
    public class RegCommandOptions : VerbAwareCommandOptions
    {
        public string path { get; set; }
        public string key { get; set; }
        public string value { get; set; }
    }
    public class RegCommand : VerbAwareCommand<RegCommandOptions>
    {
        public override string Category => CommandCategory.Core;
        public override string Description => "Manage registrykeys on the agent";
        public override string Name => "reg";

        public override CommandId CommandId => CommandId.Reg;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("verb", () =>CommandVerbs.Show.Command()).FromAmong(CommandVerbs.Show.Command(), CommandVerbs.Add.Command(), CommandVerbs.Remove.Command()),
                new Argument<string>("path", "Path of the Key"),
                new Argument<string>("key", () => string.Empty, "Name of the Key"),
                new Argument<string>("value", () => null, "Value of the Key (Add)"),
            };


        protected override void RegisterVerbs()
        {
            base.RegisterVerbs();
            this.Register(CommandVerbs.Add, this.Add);
            this.Register(CommandVerbs.Remove, this.ShowRemove);
            this.Register(CommandVerbs.Show, this.ShowRemove);
        }

        protected async Task<bool> Add(CommandContext<RegCommandOptions> context)
        {
            if (string.IsNullOrEmpty(context.Options.value))
            {
                context.Terminal.WriteError($"[X] Value is required!");
                return false;
            }

            context.AddParameter(ParameterId.Path, context.Options.path);
            context.AddParameter(ParameterId.Key, context.Options.key);
            context.AddParameter(ParameterId.Value, context.Options.value);

            return true;
        }

        protected async Task<bool> ShowRemove(CommandContext<RegCommandOptions> context)
        {
            context.AddParameter(ParameterId.Path, context.Options.path);
            context.AddParameter(ParameterId.Key, context.Options.key);

            return true;
        }
    }
}
