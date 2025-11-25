using Agent.Commands.Services;
using Agent.Communication;
using Agent.Models;
using Agent.Service;
using BinarySerializer;
using Shared;
using Shared.ResultObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinAPI;
using WinAPI.Wrapper;

namespace Agent.Commands
{
    internal class RportFwdCommand : ServiceCommand<IReversePortForwardService>
    {
        public override CommandId Command => CommandId.RportFwd;

        protected override void RegisterVerbs()
        {
            base.RegisterVerbs();
            this.Register(CommandVerbs.Start, this.Start);
            this.Register(CommandVerbs.Stop, this.Stop);
        }

        protected override async Task Show(AgentTask task, AgentCommandContext context)
        {
            var servers = this.Service.GetServers();
            if (!servers.Any())
            {
                context.AppendResult("No Reverse Port Forward running!");
                return;
            }
            var list = new List<ReversePortForwarResult>();
            foreach(var rfwd in servers)
            {
                list.Add(new ReversePortForwarResult()
                {
                    Port = rfwd.Port,
                    DestHost = rfwd.Destination.Hostname,
                    DestPort = rfwd.Destination.Port,
                });
            }
            context.Objects(list);
        }

        protected async Task Start(AgentTask task, AgentCommandContext context)
        {
            task.ThrowIfParameterMissing(ParameterId.Port);
            task.ThrowIfParameterMissing(ParameterId.Parameters);

            int port = task.GetParameter<int>(ParameterId.Port);
            ReversePortForwardDestination dest = task.GetParameter<ReversePortForwardDestination>(ParameterId.Parameters);

            if (!await this.Service.StartServer(port, context.Agent, dest))
                context.Error($"Unable to start Reverse Port Forward (port = {port})");

            return;
        }

        protected async Task Stop(AgentTask task, AgentCommandContext context)
        {
            task.ThrowIfParameterMissing(ParameterId.Port);

            int port = task.GetParameter<int>(ParameterId.Port);

            if (!await this.Service.StopServer(port))
                context.Error($"Unable to stop Reverse Port Forward (port = {port})");

            return;
        }
    }
}
