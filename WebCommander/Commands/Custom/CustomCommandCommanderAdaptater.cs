using Common.Command;
using Common.Models;
using Common.Payload;
using Shared;
using WebCommander.Commands;
using WebCommander.Models;
using WebCommander.Services;

namespace WebCommander.Commands.Custom
{
    public class CustomCommandCommanderAdaptater : ICommandCommander
    {
        private TeamServerClient _client;
        private Agent _agent;
        private CommandResult _result;

        public CommandId TargetCommandId { get; private set; }
        public string TargetCommandName { get; private set; }
        public bool Tasked { get; private set; }

        public CustomCommandCommanderAdaptater(TeamServerClient client, Agent agent, CommandResult result)
        {
            _client = client;
            _agent = agent;
            _result = result;
        }

        public void WriteError(string message)
        {
             if(string.IsNullOrEmpty(_result.Error)) _result.Error = message;
             else _result.Error += "\n" + message;
        }

        public void WriteSuccess(string message)
        {
             if(string.IsNullOrEmpty(_result.Message)) _result.Message = message;
             else _result.Message += "\n" + message;
        }

        public void WriteInfo(string message)
        {
             if(string.IsNullOrEmpty(_result.Message)) _result.Message = message;
             else _result.Message += "\n" + message;
        }

        public void WriteLine(string message)
        {
             WriteInfo(message);
        }

        public async Task<Implant> GeneratePayload(ImplantConfig options)
        {
             if(options.Endpoint != null)
                options.Endpoint = ConnexionUrl.FromString(options.Endpoint.ToString());

             var (success, creationResult) = await _client.CreateImplantAsync(options);
             if(!success)
             {
                 WriteError($"Failed to generate payload: {creationResult?.Logs}");
                 return null;
             }

             if(creationResult == null) return null;

             var implant = await _client.GetImplantWithDataAsync(creationResult.Id);

             return implant;
        }

        public void CallEndPointCommand(string commandName, CommandId commandId)
        {
            TargetCommandName = commandName;
            TargetCommandId = commandId;
            Tasked = true;
        }
    }
}
