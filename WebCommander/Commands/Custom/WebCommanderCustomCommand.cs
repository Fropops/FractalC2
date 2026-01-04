using System.Collections.Generic;
using System.CommandLine;
using System.Reflection;
using Common.Command;
using Shared;
using WebCommander.Commands;
using WebCommander.Models;

namespace WebCommander.Commands.Custom
{
    public abstract class WebCommanderCustomCommand<Comm, Opt> : CommandBase
        where Comm : CustomCommand<Opt>, new()
        where Opt : class, new()
    {
        protected Comm _customCommand;

        public override string Name => _customCommand.Name;
        public override string Description => _customCommand.Description;
        public override CommandId Id => CommandId.Script; 
        
        public override string[] Aliases => _customCommand.Alternate ?? Array.Empty<string>();
        
        public override string Category => CommandCategory.Custom;

        public override OsType[] SupportedOs => _customCommand.SupportedOs;

        public WebCommanderCustomCommand()
        {
            _customCommand = new Comm();
        }

        public override string GetUsage()
        {
            var rootCommand = CommandGenerator.GenerateRootCommand<Opt>(Description);
            rootCommand.Add(new Argument<string>(this.Name)
            {
                Arity = ArgumentArity.ExactlyOne,
            });
            return ParsedCommand.GetUsage(rootCommand);
        }

        public override async Task<CommandResult> ExecuteAsync(string commandLine)
        {
            this.CommandLine = commandLine;
            var result = new CommandResult();
            
            // 1. Parse Options
            var rootCommand = CommandGenerator.GenerateRootCommand<Opt>(Description);
            rootCommand.Add(new Argument<string>(this.Name)
            {
                Arity = ArgumentArity.ExactlyOne,
            });

            var parseResult = rootCommand.Parse(commandLine);
            
            if (parseResult.Errors.Any())
            {
                result.Failed(string.Join("\n", parseResult.Errors.Select(e => e.Message)));
                return result;
            }

            if (parseResult.Tokens.Any(t => t.Value == "-h" || t.Value == "--help" || t.Value == "/?"))
            {
                return new CommandResult().Succeed($"{this.GetUsage()}");
            }

            var options = new Opt();
            var properties = typeof(Opt).GetProperties();
            foreach (var property in properties)
            {
                var attr = property.GetCustomAttribute<CommandOptionAttribute>();
                if (attr == null) continue;

                var option = rootCommand.Options.FirstOrDefault(obj =>
                    attr.Aliases.Select(a => a.ToLower()).Contains(obj.Name.ToLower()) ||
                    obj.Aliases.Any(alias => attr.Aliases.Select(a => a.ToLower()).Contains(alias.ToLower()))
                );

                if (option != null)
                {
                    //var value = parseResult.GetValueForOption(option);
                    var value = parseResult.GetValue<object>(option.Name);
                    if (value != null)
                    {
                         property.SetValue(options, value);
                    }
                    else
                    {
                        // Use default value if not provided
                        if (attr.HasDefaultValue)
                        {
                            property.SetValue(options, attr.DefaultValue);
                        }

                    }
                }
            }


            // 2. Prepare Adapters
            var agentAdapter = new CustomCommandAgentAdaptater<Opt>(_agent?.Metadata);
            var commanderAdapter = new CustomCommandCommanderAdaptater(_client, _agent, result);
            
            var context = new CommandExecutionContext<Opt>(agentAdapter, commanderAdapter, options);
            
            // 3. Execute
            try 
            {
                var success = await _customCommand.Execute(context);
                if (!success)
                {
                    result.Failed("Execution failed. " + result.Error);
                    return result;
                }
            }
            catch(Exception ex)
            {
                result.Failed($"Exception: {ex.Message}");
                return result;
            }

            // 4. Task Agent
            if (commanderAdapter.Tasked)
            {
                // Send
                try
                {
                    var taskId = await _client.TaskAgent(Name, _agent.Id, commanderAdapter.TargetCommandId, agentAdapter.ContextParameters);
                    result.Succeed(result.Message + Environment.NewLine + $"Command {commanderAdapter.TargetCommandName} tasked to agent : {agentAdapter.Metadata?.Name?.ToString()}", taskId);
                }
                catch(Exception ex)
                {
                    result.Failed("Failed to task agent: " + ex.Message);
                }
            }
            else
            {
                // Not tasked? Maybe just local execution or failure handled silently?
                // If success is true, usually it means tasked.
                if(string.IsNullOrEmpty(result.Message)) result.Succeed("Executed.");
            }
            
            return result;
        }
    }
}
