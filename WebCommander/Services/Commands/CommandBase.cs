using System.CommandLine;
using TeamServer.UI.Models;

namespace TeamServer.UI.Services.Commands
{
    public abstract class CommandBase
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public virtual string[] Aliases { get; } = Array.Empty<string>();

        public abstract Command CreateCommand();
    }
}
