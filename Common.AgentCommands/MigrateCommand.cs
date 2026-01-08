using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;
using Common.Payload;

namespace Common.AgentCommands
{
    public class MigrateCommandOptions : CommandOption
    {
        [Option("-pid", "processId", "id of the process to injects to")]
        public int? ProcessId { get; set; }

        [Option("-b", "endpoint", "EndPoint to Bind To")]
        public string Endpoint { get; set; }

        [Option("-k", "serverKey", "The server unique key of the endpoint")]
        public string ServerKey { get; set; }

        [Option("-x86", "x86", "Generate a x86 architecture executable")]
        public bool X86 { get; set; }

        [Option("-v", "verbose", "Show details of the command execution.")]
        public bool Verbose { get; set; }
    }

    [Command("migrate", "Migrate the current agent to another process", Category = AgentCommandCategories.Execution)]
    public class MigrateCommand : AgentCommand<MigrateCommandOptions>
    {
        public override CommandId CommandId => CommandId.ForkAndRun; // Default

        public override async Task<bool> Execute(AgentCommandContext context, MigrateCommandOptions options)
        {
            var agentMetadata = context.Metadata;
            if (string.IsNullOrEmpty(options.Endpoint))
            {
                context.WriteInfo($"No Endpoint selected, taking the current agent enpoint ({agentMetadata.EndPoint})");
                options.Endpoint = agentMetadata.EndPoint;
            }

            var endpoint = ConnexionUrl.FromString(options.Endpoint);
            if (!endpoint.IsValid)
            {
                context.WriteError($"[X] EndPoint is not valid !");
                return false;
            }

            // Specify parameters manually as we bypass base.Execute's automatic calling if we override.
            // Or we call base.Execute but we need to modify the CommandId.
            
            // To change CommandId, we can't just set it.
            // We must call TaskAgent with the correct ID.
            
            // Replicate SpecifyParameters logic here or call it?
            // Since we override Execute, SpecifyParameters won't be called by base unless we call base.Execute.
            // But base.Execute uses `this.CommandId`.
            // So we must reimplement the triggering logic.
            
            this.SpecifyParameters(context, options);

            // Now capture parameters and send task.
            // But `SpecifyParameters` adds to `context` (which adds to `Adapter`).
            // `Adapter.RegisterTask` was probably called? 
            // Wait, `AgentCommandAdapter.RegisterTask` creates a NEW task every time?
            // `AgentCommandContext` delegates `AddParameter` to `Adapter`.
            // `AgentCommandAdapter.AddParameter` adds to `this.Parameters`.
            // `AgentCommandReference` logic:
            // `base.Execute` -> `CallEndPointCommand` -> `SpecifyParameters` -> `context.TaskAgent(..., this.CommandId, context.GetParameters())`.
            
            // So if I call `SpecifyParameters`, the parameters are added to `context` (Adapter.Parameters).
            // Then I call `context.TaskAgent` with the ID I want.
            
            var cmdId = options.ProcessId.HasValue ? CommandId.Inject : CommandId.ForkAndRun;
            
            // We need to pass the parameters.
            // `context.GetParameters()` should return them.
            
             context.TaskAgent(options.ToString(), cmdId, context.GetParameters());
             
             return true;
        }

        protected override void SpecifyParameters(AgentCommandContext context, MigrateCommandOptions options)
        {
            if (options.ProcessId.HasValue)
            {
                context.AddParameter(ParameterId.Id, options.ProcessId.Value);
            }
            // Add other parameters if needed?
            // "Migrate" usually implies generating a payload.
            // The `AgentCommandAdapter` or `CommModule` might handle payload generation if "Generate" task is used?
            // Original `MigrateCommand.cs` in `Commander`:
            // It calls `context.CommModule.TaskAgent`.
            // It seems it relies on `TaskAgent` to bundle parameters.
            // Does it send `Endpoint`?
            // `MigrateCommand.cs`:
            // `context.AddParameter(ParameterId.Id, context.Options.processId.Value);` (if pid)
            // `context.CommModule.TaskAgent(..., CommandId.Inject, context.Parameters);`
            // OR `CommandId.ForkAndRun`.
            // It DOES NOT seem to add Endpoint/ServerKey/Architecture to parameters?
            // Wait, if it generates a payload, the agent needs it?
            // Or `TaskAgent` handles it?
            
            // Let's re-read `MigrateCommand.cs` in `Commander` (Step 247).
            // It checks `endpoint` validity.
            // It adds `ParameterId.Id` if processId is set.
            // Then `TaskAgent`.
            // It DOES NOT add `ConnectionUrl` parameters to the *Agent Task*.
            // This is strange. How does the agent know where to connect?
            // Maybe `ForkAndRun` / `Inject` task handler on Server side generates the payload and sends it?
            // `Commander`'s `CommModule.TaskAgent` just sends the task to the DB/Server?
            // If the task content implies a payload, the Server/Agent must handle it.
            // But if parameters are missing...
            // Ah, maybe `GenericAsyncCommand` or `EnhacedCommand` does something?
            // `MigrateCommand` inherits `EnhancedCommand`.
            // But `EnhancedCommand` doesn't seem to have magic.
            
            // Perhaps `MigrateCommand` relies on the *Server* to use default config if not provided?
            // But if I specify an endpoint, it validates it but *doesn't use it* in parameters?
            // `context.Options.endpoint` is updated but not added to `context.Parameters`.
            // This looks like a bug or I am missing something in `MigrateCommand.cs`.
            // `context.Parameters` is passed to `TaskAgent`.
            
            // Wait! `MigrateCommand.cs` line 51: checks endpoint.
            // Line 58: validates endpoint.
            // Line 66: checks processId.
            // It NEVER adds `ParameterId.Endpoint` or `Meaningful Parameters` other than `Id`.
            // Unless `ParameterId` map has something?
            // Or `EnhancedCommand` adds options to parameters automatically? No.
            
            // Let's assume for now I migrate what I see.
            // But if `Endpoint` is not sent, migration might fail or use defaults hardcoded in agent?
            // Or `ForkAndRun` without params means "Clone self"?
            
            // Actually, `MigrateCommand` in `Commander` seems to default to `agent.Metadata.EndPoint`.
            // But it writes to `context.Options.endpoint`.
            // If it's not used, why validate it?
            
            // Maybe I should add `ParameterId.PipeName` or something?
            // No, let's stick to strict migration of logic:
            // - Validate Endpoint.
            // - Add `Id` if Pid is present.
            // - Send task `Inject` or `ForkAndRun`.
            
            // Wait, `Invoke-Migration` usually takes shellcode.
            // If `ForkAndRun` -> Agent expects a file/payload?
            // If I look at `ExecutePECommand`, it generated a payload.
            // `MigrateCommand` does NOT generate a payload in `Commander`.
            // Maybe the Agent *requests* the payload?
            // Or `CommandId.Inject` implies "Inject *Me*"?
            
            // I will strictly replicate logic.
        }
    }
}
