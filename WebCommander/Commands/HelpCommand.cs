using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Common.AgentCommands;
using Common.CommandLine.Core;
using Common.CommandLine.Execution;
using WebCommander.Commands;
using Shared;

namespace WebCommander.Commands
{
    [Command("help", "Give info on available commands", Category = "Commander")]
    public class HelpCommand : ICommand<AgentCommandContext, CommandOption>
    {
        public async Task<bool> Execute(AgentCommandContext context, CommandOption options)
        {
            Console.WriteLine($"[Terminal] Context ok");
            var adapter = context.Adapter as WebAgentCommandAdapter;

            if (adapter != null)
            {
                var cmds = adapter.GetAvailableCommands();

                context.WriteLine("Available commands :");

                var categories = new List<string> { "Commander" };
                var availableCommands = new List<CommandDefinition>();

                var currentAgentOs = context.Metadata.OsType;

                foreach (var cmdDef in cmds)
                {
                    if (typeof(Common.AgentCommands.AgentCommandBase).IsAssignableFrom(cmdDef.CommandType))
                    {
                        // Check OS support
                        var cmd = (AgentCommandBase)Activator.CreateInstance(cmdDef.CommandType);
                        if (cmd.SupportedOs.Contains(currentAgentOs))
                        {
                            availableCommands.Add(cmdDef);
                        }
                    }
                    else
                    {
                        // Assume other commands (like Upload or Help) are available
                        availableCommands.Add(cmdDef);
                    }
                }

                categories.AddRange(availableCommands
                    .Select(c => c.CommandType.GetCustomAttribute<CommandAttribute>()?.Category ?? "Uncategorized")
                    .Distinct()
                    .Where(c => c != "Commander")
                    .OrderBy(cat => cat));

                foreach (var cat in categories)
                {
                    var tmpCmds = availableCommands.Where(c => (c.CommandType.GetCustomAttribute<CommandAttribute>()?.Category ?? "Uncategorized") == cat);

                    if (!tmpCmds.Any())
                        continue;

                    context.WriteInfo($"--- {cat} ---");

                    // Simple text formatting
                    var maxNameLen = tmpCmds.Max(c => c.CommandType.GetCustomAttribute<CommandAttribute>()?.Name.Length ?? 0) + 2;
                    var cmdList = string.Empty;
                    foreach (var cmd in tmpCmds.OrderBy(c => c.CommandType.GetCustomAttribute<CommandAttribute>()?.Name))
                    {
                        var attr = cmd.CommandType.GetCustomAttribute<CommandAttribute>();
                        var name = attr?.Name ?? "Unknown";
                        var desc = attr?.Description ?? "";

                        cmdList += $"{name.PadRight(maxNameLen)} : {desc}\n";
                    }
                    context.WriteLine(cmdList);
                    context.WriteLine(""); // empty line
                }
            }

                return true;
        }
    }
}