using Commander.Communication;
using Commander.Executor;
using Commander.Helper;
using Commander.Terminal;
using Shared;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Agent
{
    public class ViewTasksCommandOptions
    {
        public int? index { get; set; }
        public int? Top { get; set; }
        public bool verbose { get; set; }
    }

    public class ViewTasksCommand : EnhancedCommand<ViewTasksCommandOptions>
    {
        public override string Category => CommandCategory.Commander;
        public override string Description => "List all task of an agent, or the detail of a specific task";
        public override string Name => "view";

        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<int?>("index", "index of the task to view"),
                new Option<int?>(new[] { "--top", "-t" }, "The max number of taks retrieved (only when no index is set. Default is 10."),
                //new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
            };


        protected override async Task<bool> HandleCommand(CommandContext<ViewTasksCommandOptions> context)
        {


            if (context.Options.index.HasValue)
            {
                //Show Result of the Task
                var task = context.CommModule.GetTasks(context.Executor.CurrentAgent.Id).Skip(context.Options.index.Value).FirstOrDefault();
                if (task == null)
                {
                    context.Terminal.WriteError($"No task at index {context.Options.index}");
                    return true;
                }

                var result = context.CommModule.GetTaskResult(task.Id);
                if (result == null)
                    context.Terminal.WriteInfo($"Task : {task.Command} is queued.");
                else
                    TaskPrinter.Print(task, result, context.Terminal);

                return true;
            }
            else
            {
                var table = new Table();
                table.Border(TableBorder.Rounded);
                // Add some columns
                table.AddColumn(new TableColumn("Index").Centered());
                table.AddColumn(new TableColumn("Id").LeftAligned());
                table.AddColumn(new TableColumn("Command").LeftAligned());
                //table.AddColumn(new TableColumn("Info").LeftAligned());
                table.AddColumn(new TableColumn("Status").LeftAligned());
                table.AddColumn(new TableColumn("Date").LeftAligned());


                int take = context.Options.Top ?? 10;
                var tasks = context.CommModule.GetTasks(context.Executor.CurrentAgent.Id).Take(take);

                if (tasks.Count() == 0)
                {
                    context.Terminal.WriteInfo("No Tasks.");
                    return true;
                }

                var index = 0;
                foreach (var task in tasks)
                {
                    var result = context.CommModule.GetTaskResult(task.Id);

                    table.AddRow(
                        index.ToString(),
                        task.Id,
                        task.Command ?? string.Empty,
                        //Arguments = task.Arguments,
                        //result == null ? string.Empty : result.Info ?? string.Empty,
                        result == null ? AgentResultStatus.Queued.ToString() : result.Status.ToString(),
                        task.RequestDate.ToLocalTime().ToString()
                    );
                    index++;
                }

                table.Expand();
                context.Terminal.Write(table);

                return true;
            }

        }
    }


}
