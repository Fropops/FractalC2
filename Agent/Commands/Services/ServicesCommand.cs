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
	public class LinkCommand : AgentCommand
	{
		public override string Name => "service";

		public override void InnerExecute(AgentTask task, AgentCommandContext context)
		{

			var list = new SharpSploitResultList<ListServicesResult>();
			foreach (var service in ServiceProvider.GetRunningServices())
			{
				list.Add(new ListServicesResult()
				{
					Service = service.ServiceName,
					Status = service.Status == RunningStatus.Running ? "[Running]" : "[Off]"
				});

			}
			context.Result.Result = list.ToString();
			return;

		}


		public sealed class ListServicesResult : SharpSploitResult
		{

			public string Service { get; set; }
			public string Status { get; set; }

			protected internal override IList<SharpSploitResultProperty> ResultProperties => new List<SharpSploitResultProperty>()
			{
				new SharpSploitResultProperty { Name = nameof(Service), Value = Service },
				new SharpSploitResultProperty { Name = nameof(Status), Value = Status },
			};
		}
	}
}
