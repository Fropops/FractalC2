using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;

namespace Common.AgentCommands
{
    public class LinkCommandOptions : CommandOption
    {
        [Argument("Verb", "actions available", 0, DefaultValue = CommandVerbs.Show, AllowedValues = new object[] { CommandVerbs.Show, CommandVerbs.Start, CommandVerbs.Stop }, IsRequired = true)]
        public string Verb { get; set; }

        [Option("b", "bindto", "Endpoint To bind to")]
        public string BindTo { get; set; }
    }

    [Command("link", "Link to another Agent", Category = AgentCommandCategories.System)]
    public class LinkCommand : AgentCommand<LinkCommandOptions>
    {
        public override CommandId CommandId => CommandId.Link;

        protected override bool CheckParams(AgentCommandContext context, LinkCommandOptions options)
        {
             if (options.Verb.Equals("start", StringComparison.OrdinalIgnoreCase) || options.Verb.Equals("stop", StringComparison.OrdinalIgnoreCase))
             {
                 ConnexionUrl conn = ConnexionUrl.FromString(options.BindTo);
                if (conn == null || !conn.IsValid)
                {
                    context.WriteError($"BindTo is not valid!");
                    return false;
                }
             }

            return base.CheckParams(context, options);
        }

        protected override void SpecifyParameters(AgentCommandContext context, LinkCommandOptions options)
        {
            CommandVerbs verb = (CommandVerbs)Enum.Parse(typeof(CommandVerbs), options.Verb, true);
            context.AddParameter(ParameterId.Verb, verb);

            if (!string.IsNullOrEmpty(options.BindTo) && !options.Verb.Equals("show", StringComparison.OrdinalIgnoreCase))
            {
                ConnexionUrl conn = ConnexionUrl.FromString(options.BindTo);
                context.AddParameter(ParameterId.Bind, conn.ToString());
            }
        }
    }
}
