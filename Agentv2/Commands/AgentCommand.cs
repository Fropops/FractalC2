using Agent.Communication;
using Agent.Models;
using Agent.Service;
using BinarySerializer;
using Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class AgentCommandContext
    {
        public bool IsScripting { get; set; } = false;

        public Agent Agent { get; set; }

        public INetworkService NetworkService { get; set; }

        public IFileService FileService { get; set; }

        public IConfigService ConfigService { get; set; }
        //public IProxyService ProxyService { get; set; }

        public AgentTaskResult Result { get; set; }

        public CancellationTokenSource TokenSource { get; set; }


        public void AppendResult(string message, bool addEndLine = true)
        {
            if (string.IsNullOrEmpty(this.Result.Output))
                Result.Output = message;
            else
                Result.Output += message;

            if (addEndLine)
                Result.Output += Environment.NewLine;
        }

        public void ClearResult()
        {
            Result.Output = string.Empty;
        }

        public void Error(string message, bool addEndLine = true)
        {
            if (string.IsNullOrEmpty(this.Result.Error))
                Result.Error = message;
            else
                Result.Error += message;

            if (addEndLine)
                Result.Error += Environment.NewLine;
        }

        public void Objects(byte[] data)
        {
            this.Result.Objects = data;
        }

        public void Objects<T>(T item)
        {
            this.Result.Objects = item.BinarySerializeAsync().Result;
        }
    }

    public abstract class AgentCommand
    {
        public int? JobId { get; set; }
        public AgentCommandContext Context { get; set; }

        bool IsScriptCommand
        {
            get
            {
                return this.Command == CommandId.Script;
            }
        }

        protected bool SendMetadataWithResult = false;
        public virtual CommandId Command { get; protected set; }

        public bool Threaded { get; protected set; } = true;

        public CancellationToken CancellationToken { get; private set; }

        public virtual async Task Execute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            this.Context = context;
            context.Result.Id = task.Id;
            try
            {
#if DEBUG
                Debug.WriteLine($"Executing {task.CommandId} ...");
#endif
                context.Result.Status = AgentResultStatus.Running;
                if (!context.IsScripting  || this.IsScriptCommand) //sending will be handled in the composite command
                    await context.Agent.SendTaskResult(context.Result);
                await this.InnerExecute(task, context, token);
            }
            catch (Exception e)
            {
                //context.Result.Result = "An unhandled error occured :" + Environment.NewLine;
                //context.Result.Result += e.ToString();
#if DEBUG
                context.Result.Error = e.ToString();
#else
                context.Result.Error = e.Message;
#endif
                context.Result.Error += Environment.NewLine;
            }
            finally
            {
                if (!context.IsScripting  || this.IsScriptCommand) //sending will be handled in the composite command
                {
                    context.Result.Info = string.Empty;
                    if (!string.IsNullOrEmpty(context.Result.Error))
                        context.Result.Status = AgentResultStatus.Error;
                    else
                        context.Result.Status = AgentResultStatus.Completed;

                    await context.Agent.SendTaskResult(context.Result);
                }

                if (JobId.HasValue)
                    ServiceProvider.GetService<IJobService>().RemoveJob(JobId.Value);

                if (context.Agent.TaskTokens.ContainsKey(task.Id))
                {
                    context.Agent.TaskTokens[task.Id].Dispose();
                    context.Agent.TaskTokens.Remove(task.Id);
                }

            }
#if DEBUG
            Debug.WriteLine($"{task.CommandId} Executed ({context.Result.Status}).");
#endif

        }

        public abstract Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token);

        //public void Notify(AgentCommandContext context, string status)
        //{
        //    if (context.ParentContext == null)
        //        context.Agent.SendResult(new AgentTaskResult()
        //        {
        //            Id = context.Result.Id,
        //            Status = context.Result.Status,
        //            Info = status
        //        });
        //    else
        //    {
        //        context.ParentContext.Agent.SendResult(new AgentTaskResult()
        //        {
        //            Id = context.ParentContext.Result.Id,
        //            Status = context.ParentContext.Result.Status,
        //            Info = status
        //        });
        //    }
        //}


        //protected void CheckFileDownloaded(AgentTask task, AgentCommandContext context)
        //{
        //    int percent = -1;
        //    while (!context.FileService.IsDownloadComplete(task.FileId))
        //    {
        //        var newpercent = context.FileService.GetDownloadPercent(task.FileId);
        //        if (newpercent != percent)
        //        {
        //            percent = newpercent;
        //            this.Notify(context, $"{task.FileName} Downloading {percent}%");
        //        }

        //        if (percent != 100)
        //            Thread.Sleep(context.Agent.Communicator.MessageService.AgentMetaData.SleepInterval);
        //    }

        //    this.Notify(context, $"{task.FileName} Downloaded");

        //}

        //protected void CheckFileUploaded(string fileId, string fileName, AgentCommandContext context)
        //{
        //    int percent = -1;
        //    while (!context.FileService.IsUploadComplete(fileId))
        //    {
        //        var newpercent = context.FileService.GetUploadPercent(fileId);
        //        if (newpercent != percent)
        //        {
        //            percent = newpercent;
        //            this.Notify(context, $"{fileName} uploading {percent}%");
        //        }

        //        if (percent != 100)
        //            Thread.Sleep(context.Agent.Communicator.MessageService.AgentMetaData.SleepInterval);
        //    }

        //    this.Notify(context, $"{fileName} uploaded");

        //}
    }
}
