using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands
{
    public class Base64Command : ExecutorCommand
    {
        public override string Name => "b64";

        public override string Description => "Encode to base 64";
        public override string Category => CommandCategory.Core;
        public override Executor.ExecutorMode AvaliableIn => Executor.ExecutorMode.All;
        protected override void InnerExecute(CommandContext context)
        {
            if(string.IsNullOrEmpty(context.CommandParameters))
            {
                context.Terminal.WriteLine($"Usage : {this.Name} <string to encode>");
                return;
            }
            var input = context.CommandParameters;
            var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
            context.Terminal.WriteLine(b64);
        }
    }
}
