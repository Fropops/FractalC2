using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Shared;
using WebCommander.Models;

namespace WebCommander.Commands
{
    public abstract class VerbAwareCommand : EndPointCommand
    {

        protected string verbParam = "Verb";

        protected abstract List<CommandVerbs> AllowedVerbs { get; }
        protected override void AddCommandParameters(RootCommand command)
        {
            command.Arguments.Add(new Argument<string>(verbParam) { Arity = ArgumentArity.ExactlyOne, Description = "The action to perform" });
        }

        public override async Task FillParametersAsync(ParseResult parseResult, ParameterDictionary parms)
        {
            Console.WriteLine("In Verb Aware Command");
            string verbStr = CamelCase(parseResult.GetValue<string>(verbParam));
            CommandVerbs verb;
            try
            {
                verb = (CommandVerbs)Enum.Parse(typeof(CommandVerbs), verbStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Invalid verb: {verbStr}");
            }
            if (!AllowedVerbs.Contains(verb))
                throw new ArgumentException($"Invalid verb: {verb.ToString()}");
            parms.AddParameter(ParameterId.Verb, verb);
        }

        static string CamelCase(string texte)
        {
            if (string.IsNullOrEmpty(texte))
                return texte;
            
            if (texte.Length == 1)
                return texte.ToUpper();
            
            return char.ToUpper(texte[0]) + texte.Substring(1).ToLower();
        }

         public override string GetUsage()
        {
            string usage = base.GetUsage() + Environment.NewLine;
            usage += "Verb is required. Valid values are: " + string.Join(", ", AllowedVerbs);
            return usage;
        }
    }
}
