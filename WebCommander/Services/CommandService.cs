using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Common.AgentCommands;
using Common.CommandLine.Execution;
using WebCommander.Commands;
using WebCommander.Models;

namespace WebCommander.Services
{
    public class CommandService
    {
        private readonly TeamServerClient _client;
        private readonly CommandExecutor _commandExecutor;
        
        // This property allows passing file bytes for the current command execution context
        private byte[]? _currentFileBytes;
        private Agent? _currentAgent;

        public CommandService(TeamServerClient client)
        {
            _client = client;
            _commandExecutor = new CommandExecutor();
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            // Register Context Factories
             
            // Register the WebAgentCommandAdapter factory
            _commandExecutor.RegisterContextFactory(() => 
            {
                if (_currentAgent == null)
                    throw new InvalidOperationException("No current agent set for command execution context.");
                    
                var adapter = new WebAgentCommandAdapter(_client, _currentAgent, this, _currentFileBytes);
                if (_currentFileBytes != null)
                {
                    adapter.AddParameter(Shared.ParameterId.File, _currentFileBytes);
                }
                    
                return new AgentCommandContext(adapter);
            });

            // Load Common Commands
            // We need to load commands from Common assembly (e.g. WhoamiCommand)
            var commonAssembly = typeof(WhoamiCommand).Assembly;
            _commandExecutor.LoadCommands(commonAssembly);
            
            // Load WebCommander Commands (e.g. UploadCommand)
            var webAssembly = Assembly.GetExecutingAssembly();
            _commandExecutor.LoadCommands(webAssembly);
        }

        public async Task<CommandResult> ParseAndSendAsync(string rawInput, Agent agent, object complement = null)
        {
            if (string.IsNullOrWhiteSpace(rawInput))
                return null;

            _currentAgent = agent;

            try
            {
                var result = await _commandExecutor.ExecuteAsync(rawInput, complement);
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                _currentAgent = null;
                _currentFileBytes = null;
            }
        }
        
        public List<CommandDefinition> GetCommands()
        {
            return _commandExecutor.RegisteredCommands;
        }
    }
}
