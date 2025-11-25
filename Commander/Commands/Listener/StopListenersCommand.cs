using Commander.Executor;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;

namespace Commander.Commands.Listener
{
    public class StopListenerCommandOptions
    {
        public string name { get; set; }
        public bool clean { get; set; } = false;

    }


    public class StopListenerCommand : EnhancedCommand<StopListenerCommandOptions>
    {
        public override string Category => CommandCategory.Listeners;
        public override string Description => "Stop a listener";
        public override string Name => "stop";
        public override ExecutorMode AvaliableIn => ExecutorMode.Listener;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("name", "name of the listener"),
                new Option(new[] { "--clean", "-c" }, "Show details of the command execution."),
            };

        protected override async Task<bool> HandleCommand(CommandContext<StopListenerCommandOptions> context)
        {
            var listener = context.CommModule.GetListeners().FirstOrDefault(l => l.Name.ToLower().Equals(context.Options.name.ToLower()));
            if (listener == null)
            {
                context.Terminal.WriteError($"Cannot find listener whith the name {context.Options.name} !");
                return false;
            }

            var result = await context.CommModule.StopListener(listener.Id, context.Options.clean);
            if (!result.IsSuccessStatusCode)
            {
                context.Terminal.WriteError("An error occured : " + result.StatusCode);
                return false;
            }

            context.Terminal.WriteSuccess($"Listener {listener.Name} stoped.");
            return true;
        }
    }

}
