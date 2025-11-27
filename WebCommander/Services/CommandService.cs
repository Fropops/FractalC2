using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WebCommander.Models;
using WebCommander.Services.Commands;

namespace WebCommander.Services
{
    public class CommandService
    {
        private readonly TeamServerClient _client;
        private readonly Dictionary<string, Type> _commandMap = new();

        public CommandService(TeamServerClient client)
        {
            _client = client;
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            var commandTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(CommandBase)) && !t.IsAbstract);

            foreach (var type in commandTypes)
            {
                Console.WriteLine($"Loading command {type.Name}");
                var commandBase = CreateCommand(type);
                if (commandBase != null)
                {
                    Console.WriteLine($"Command {type.Name} loaded");
                    _commandMap[commandBase.Name] = type;
                    foreach (var alias in commandBase.Aliases)
                    {
                        _commandMap[alias] = type;
                    }
                }
            }
        }

        public CommandBase GetCommand(string cmdName, string agentId = null)
        {
            if (_commandMap.TryGetValue(cmdName, out var commandType))
            {
                return CreateCommand(commandType, agentId);
            }
            Console.WriteLine($"Command {cmdName} not found");
            return null;
        }

        public CommandBase CreateCommand(Type type, string agentId = null)
        {
            if (Activator.CreateInstance(type) is CommandBase commandBase)
            {
                commandBase.Initialize(_client, agentId);
                return commandBase;
            }
            Console.WriteLine($"Command {type.Name} not created");
            return null;
        }

        public async Task<(string? message, string? error, string? taskId)> ParseAndSendAsync(string rawInput, string agentId)
        {
            if (string.IsNullOrWhiteSpace(rawInput))
                return (null, null, null);

            //juste pour récupérer le nom de la commande qui nous intéresse
            var parseResult = new RootCommand().Parse(rawInput);
            string cmdName = parseResult.Tokens.FirstOrDefault().Value;

            Console.WriteLine($"Command: {cmdName}");
            var commandBase = GetCommand(cmdName, agentId);
            if (commandBase != null)
            {
                Console.WriteLine($"Command: {commandBase.Name} created");
                if(commandBase is EndPointCommand)
                {
                    
                    parseResult = commandBase.Parse(rawInput);
                    // Handle Help or Errors
                    if (parseResult.Errors.Count > 0 || parseResult.Tokens.Any(t => t.Value == "-h" || t.Value == "--help" || t.Value == "/?"))
                    {
                        var cmd = parseResult.CommandResult.Command;
                        var errorMsg = parseResult.Errors.Count > 0 ? $"{string.Join(", ", parseResult.Errors.Select(e => e.Message))}\n" : "";
                        return ($"{commandBase.GetUsage()}",errorMsg, null);
                    }

                    await parseResult.InvokeAsync();
                    var cmdResult = commandBase.Result;
                    Console.WriteLine($"Command: {cmdName} executed");
                    return (cmdResult.Message, cmdResult.Error, cmdResult.TaskId);
                }
                if(commandBase is ShellEndPointCommand)
                {
                    var cmdResult = await (commandBase as ShellEndPointCommand).ExecuteAsync(cmdName, rawInput, _client, agentId);
                    return (cmdResult.Message, cmdResult.Error, cmdResult.TaskId);
                }
            }

            return (null, $"[Error] Unknown command or not implemented.", null);
        }
    }
}
