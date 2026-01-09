using Commander.Commands;
using Commander.Communication;
using Commander.Executor;
using Commander.Helper;
using Commander.Terminal;
using Common.CommandLine.Core;
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
    public class ViewTasksCommandOptions : CommandOption
    {
        [Argument("index", "index of the task to view", 0)]
        public int? index { get; set; }
        [Option("-t", "--top", "The max number of taks retrieved (only when no index is set. Default is 10.)", DefaultValue = 10)]
        public int? Top { get; set; }
        [Option("-l", "--loot", "Save the task result to loot.")]
        public bool loot { get; set; }
    }

    [Command("view", "List all task of an agent, or the detail of a specific task", Category = "Commander")]
    public class ViewTasksCommand : ICommanderAgentCommand, ICommand<CommanderCommandContext, ViewTasksCommandOptions>
    {
        public async Task<bool> Execute(CommanderCommandContext context, ViewTasksCommandOptions options)
        {
            if (options.index.HasValue)
            {
                //Show Result of the Task
                var task = context.CommModule.GetTasks(context.Executor.CurrentAgent.Id).Skip(options.index.Value).FirstOrDefault();
                if (task == null)
                {
                    context.Terminal.WriteError($"No task at index {options.index}");
                    return true;
                }

                var result = context.CommModule.GetTaskResult(task.Id);
                if (result == null)
                    context.Terminal.WriteInfo($"Task : {task.Command} is queued.");
                else
                {
                    TaskPrinter.Print(task, result, context.Terminal);

                    if (options.loot)
                    {
                        try
                        {
                            var content = await LootOutputFormatter.FormatLootContent(context.Executor.CurrentAgent, task, result);
                            var fileName = $"task_{task.Id}.txt";
                            var loot = new Common.APIModels.Loot
                            {
                                AgentId = context.Executor.CurrentAgent.Id,
                                FileName = fileName,
                                Data = Convert.ToBase64String(content),
                                IsImage = false
                            };

                            var success = await context.CommModule.CreateLootAsync(context.Executor.CurrentAgent.Id, loot);
                            if (success)
                                context.Terminal.WriteSuccess($"Task output saved to loot as {fileName}");
                            else
                                context.Terminal.WriteError("Failed to save loot.");
                        }
                        catch (Exception ex)
                        {
                            context.Terminal.WriteError($"Error saving loot: {ex.Message}");
                        }
                    }
                }

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


                int take = options.Top ?? 10;
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
