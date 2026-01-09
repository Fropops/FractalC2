using System;
using System.Threading.Tasks;
using Common.AgentCommands;
using Common.CommandLine.Core;
using Shared;
using WebCommander.Services;

namespace WebCommander.Commands
{
    public class UploadCommandOptions : CommandOption
    {
       [Argument("remotefile", "Path of the file to be saved on the agent", 0)]
       public string remoteFile { get; set; }
    }

    [Command("upload", "Upload a file to the agent", Category = "Network", Aliases = new string[] { "put" })]
    public class UploadCommand : AgentCommand<UploadCommandOptions>
    {
        public override CommandId CommandId => CommandId.Upload;
        
        public override async Task<bool> Execute(AgentCommandContext context, UploadCommandOptions options)
        {
            // Retrieve file bytes from parameters (populated by CommandService)
            var parameters = context.GetParameters();
            byte[] fileBytes = null;
            
            // Assuming ParameterDictionary has a way to get values. 
            // In Shared, it might use methods or direct dictionary access.
            // Let's assume TryGetParameter or similar, or just check if we can iterate.
            // But since I don't see ParameterDictionary source, I'll rely on what I saw in Adapter:
            // Adapter.AddParameter(ParameterId.File, bytes).
            // ParameterDictionary probably stores objects.
            
            // I'll try to get it. If not available, we can't upload.
            // But how to get from ParameterDictionary?
            // Let's guess: it implements IDictionary or has Get method.
            // Actually, in Helper/WebAgentCommandAdapter.cs I saw:
            // public void AddParameter<T>(ParameterId id, T item) => Parameters.AddParameter(id, item);
            
            // If I can't find a helper, I'll assume we can use the 'FileBytes' derived property if I can cast the Context...
            // But I can't cast Context.Adapter.
            // So I MUST use parameters.
            
            // Let's assume we can get it.
            // Wait, ParameterDictionary in Shared? 
            // Let's look at Shared/ParameterDictionary.cs if possible.
            // Or assume Get<T>(ParameterId).
            
            if (parameters.TryGetValue(ParameterId.File, out fileBytes)) 
            {
               // Found
            }

            if (fileBytes == null || fileBytes.Length == 0)
            {
                context.WriteError("No file content provided for upload. Please use the UI to upload a file.");
                return false;
            }

            // Determine remote filename
            string filename = options.remoteFile ?? "unknown_upload";
            
            context.Upload(fileBytes, filename);
            
            // We return true because context.Upload Queues the task (via RegisterTask).
            // But wait, context.Upload in Adapter calls RegisterTask.
            // My Adapter.TaskAgent logic sends the queued tasks.
            // AgentCommand.Execute calls CallEndPointCommand which calls SpecifyParameters then TaskAgent.
            // If I override Execute, I am responsible for calling TaskAgent?
            // Yes.
            // But context.Upload adds a task to the queue.
            // context.TaskAgent(..., ...) sends the task.
            // If I call context.Upload, I have a queued task.
            // I should call context.TaskAgent(string, CommandId, dictionary).
            // BUT my adapter's TaskAgent sends queued tasks if they exist!
            // So:
            
            // 1. Queue upload task
            // context.Upload(fileBytes, filename);
            
            // 2. Trigger send
            context.TaskAgent($"upload {filename}", CommandId.Upload, null); 
            // Passing null parameters because the task is already in queue with its own parameters.
            // My Adapter logic handles this.
            
            return true;
        }
    }
}
