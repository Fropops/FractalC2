using Commander.Executor;
using Common.Models;
using Newtonsoft.Json;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;

namespace Commander.Commands.Listener
{
    public class CreateListenersCommandOptions
    {
        public string name { get; set; }
        public bool verbose { get; set; }
        public int? port { get; set; }

        public string address { get; set; }
        public bool secured { get; set; }

    }


    public class InteractAgentCommand : EnhancedCommand<CreateListenersCommandOptions>
    {
        public override string Category => CommandCategory.Listeners;
        public override string Description => "Start a new listener";
        public override string Name => "start";
        public override ExecutorMode AvaliableIn => ExecutorMode.Listener;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("name", "name of the listener"),
                new Option<string>(new[] { "--address", "-a" }, () => "*", "The listening address."),
                new Option<int?>(new[] { "--port", "-p" }, () => null, "The listening port."),
                new Option<bool>(new[] { "--secured", "-s" }, () => true, "HTTPS if secured else HTTP"),
                new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
            };

        protected override async Task<bool> HandleCommand(CommandContext<CreateListenersCommandOptions> context)
        {


            if(!context.Options.port.HasValue)
            {
                if (context.Options.secured)
                    context.Options.port = 443;
                else
                    context.Options.port = 80;
            }



            if (context.CommModule.GetListeners().Any(l => l.Name.ToLower().Equals(context.Options.name.ToLower())))
            {
                context.Terminal.WriteError($"A listener whith the name {context.Options.name} already exists !");
                return false;
            }

            var result = await context.CommModule.CreateListener(context.Options.name, context.Options.port.Value, context.Options.address, context.Options.secured);
            if (!result.IsSuccessStatusCode)
            {
                context.Terminal.WriteError("An error occured : " + result.StatusCode);
                return false;
            }

            var json = await result.Content.ReadAsStringAsync();
            var listener = JsonConvert.DeserializeObject<TeamServerListener>(json);

            context.Terminal.WriteSuccess($"Listener {listener.Name} started on port {listener.BindPort}.");
            return true;
        }
    }

}
