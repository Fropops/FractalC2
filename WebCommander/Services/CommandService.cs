using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TeamServer.UI.Models;
using TeamServer.UI.Services.Commands;

namespace TeamServer.UI.Services
{
    public class CommandService
    {
        private readonly TeamServerClient _client;
        private readonly RootCommand _rootCommand;
        private readonly Dictionary<Command, CommandBase> _commandMap = new();

        public CommandService(TeamServerClient client)
        {
            _client = client;
            _rootCommand = new RootCommand("TeamServer Agent Terminal");
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            var commandTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(CommandBase)) && !t.IsAbstract);

            foreach (var type in commandTypes)
            {
                if (Activator.CreateInstance(type) is CommandBase commandBase)
                {
                    var command = commandBase.CreateCommand();
                    _rootCommand.Add(command);
                    _commandMap[command] = commandBase;
                }
            }
        }

        public async Task<(string message, string? taskId)> ParseAndSendAsync(string rawInput, string agentId)
        {
            if (string.IsNullOrWhiteSpace(rawInput))
                return (string.Empty, null);

            var result = _rootCommand.Parse(rawInput);

            // Handle Help or Errors
            if (result.Errors.Count > 0 || result.Tokens.Any(t => t.Value == "-h" || t.Value == "--help" || t.Value == "/?"))
            {
                 var cmd = result.CommandResult.Command;
                 var errorMsg = result.Errors.Count > 0 ? "Invalid parameters.\n" : "";
                 return ($"{errorMsg}Command: {cmd.Name}\nDescription: {cmd.Description}\nUsage: {cmd.Name} [arguments]", null);
            }

            var commandResult = result.CommandResult;
            var command = commandResult.Command;

            if (_commandMap.TryGetValue(command, out var commandBase))
            {
                if (commandBase is EndPointCommand endPointCommand)
                {
                    return await endPointCommand.ExecuteAsync(result, _client, agentId);
                }
                else if (commandBase is ExecuteCommand)
                {
                    return ("ExecuteCommand not implemented yet.", null);
                }
            }

            return ($"[Error] Unknown command or not implemented.", null);
        }
    }
}
