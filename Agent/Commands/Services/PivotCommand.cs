using Agent.Commands.Services;
using Agent.Communication;
using Agent.Models;
using Agent.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Agent.Service.RunningService;

namespace Agent.Commands
{
    public class PivotCommand : ServiceCommand<IPivotService>
    {
        public override string Name => "pivot";

        protected override void Start(AgentTask task, AgentCommandContext context, string[] args)
        {
            if (this.Service.Status == RunningStatus.Stoped)
            {
                this.Service.Start();
            }

            var url = args[0].ToLower();
            var conn = ConnexionUrl.FromString(url);
            if (!conn.IsValid)
            {
                context.Result.Result = $"Pivot binding is not valid !";
                return;
            }

            if (conn.IsSecure && conn.Protocol == ConnexionType.Http)
            {
                context.Result.Result = $"Pivot Https is not supported !";
                return;
            }



            if (!this.Service.AddPivot(conn, context.ConfigService.ServerKey))
            {
                context.Result.Result = $"Cannot start pivot {conn}!";
                return;
            }


            context.Result.Result = $"Pivot {conn.ToString()} started";
        }

        protected override void Stop(AgentTask task, AgentCommandContext context, string[] args)
        {
            var url = task.SplittedArgs[1].ToLower();
            var conn = ConnexionUrl.FromString(url);
            this.Service.RemovePivot(conn);
            context.Result.Result = $"Pivot {conn} stopped.";

            if (!this.Service.HasPivots())
                this.Service.Stop();
        }

        protected override void Show(AgentTask task, AgentCommandContext context, string[] args)
        {
            if (!this.Service.Pivots.Any())
            {
                context.Result.Result = "No pivots configured!";
                return;
            }

            var list = new SharpSploitResultList<ListPivotsResult>();
            foreach (var pivot in this.Service.Pivots)
            {
                list.Add(new ListPivotsResult()
                {
                    Type = pivot.Connexion.ProtocolString,
                    BindTo = pivot.Connexion.ToString(),
                });

            }
            context.Result.Result = list.ToString();
            return;
        }

        public sealed class ListPivotsResult : SharpSploitResult
        {

            public string Type { get; set; }
            public string BindTo { get; set; }

            protected internal override IList<SharpSploitResultProperty> ResultProperties => new List<SharpSploitResultProperty>()
            {
                new SharpSploitResultProperty { Name = nameof(Type), Value = Type },
                new SharpSploitResultProperty { Name = nameof(BindTo), Value = BindTo },
            };
        }
    }
}
