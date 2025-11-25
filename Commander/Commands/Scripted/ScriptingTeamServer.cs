using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Commander.Commands.Scripted
{
    public class ScriptingTeamServer<T>
    {
        private CommandContext<T> context;

        public ScriptingTeamServer(CommandContext<T> ctxt)
        {
            context = ctxt;
        }
    }
}
