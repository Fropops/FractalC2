using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Commander.Models;
using Spectre.Console;
using Commander.Commands.Agent;
using Shared;
using System.Runtime.ConstrainedExecution;
using Common.APIModels.WebHost;
using Common.Models;
using Commander.Helper;

namespace Commander.Commands.Network
{
    public class WebHostCommandOptions : VerbAwareCommandOptions
    {
        public string path { get; set; }
        public string file { get; set; }
        public bool powershell { get; set; }
        public string description { get; set; }

        public string listener { get; set; }
    }

    public class WebHostCommand : VerbAwareCommand<WebHostCommandOptions>
    {
        public override string Category => CommandCategory.Network;
        public override string Description => "WebHost file on the TeamServer";
        public override string Name => "host";

        public override ExecutorMode AvaliableIn => ExecutorMode.All;

        public override RootCommand Command => new RootCommand(Description)
            {
                new Argument<string>("verb", () => CommandVerbs.Show.Command()).FromAmong(CommandVerbs.Push.Command(), CommandVerbs.Remove.Command(), CommandVerbs.Show.Command(), CommandVerbs.Script.Command(), CommandVerbs.Log.Command(), CommandVerbs.Clear.Command()),

                new Option<string>(new[] { "--file", "-f" }, () => null, "Path of the local file to push (" + CommandVerbs.Push.Command() + ")"),
                new Option<string>(new[] { "--path", "-p" }, () => null, "Hosting path (" + CommandVerbs.Push.Command() + "," + CommandVerbs.Show.Command() + ")"),
                new Option<bool>(new[] { "--powershell", "-ps" }, () => false, "Specify is the file is a powershell script (" + CommandVerbs.Push.Command() + ")"),
                new Option<string>(new[] { "--description", "-d" }, () => null, "Description of the file (" + CommandVerbs.Push.Command() + ")"),
                new Option<string>(new[] { "--listener", "-l" }, () => null, "filter on specific listener (" + CommandVerbs.Show.Command() + ")"),
            };

        public string[] VerbsToList(params CommandVerbs[] verbs)
        {
            return verbs.Select(c => c.Command()).ToArray();
        }

        protected override void RegisterVerbs()
        {

            base.RegisterVerbs();
            Register(CommandVerbs.Show, Show);
            Register(CommandVerbs.Push, Push);
            Register(CommandVerbs.Remove, Remove);
            Register(CommandVerbs.Log, Log);
            Register(CommandVerbs.Clear, Clear);
            Register(CommandVerbs.Script, Show);
        }

        protected override async Task CallEndPointCommand(CommandContext<WebHostCommandOptions> context)
        {
                //don't call endpoint as it's a server side functionality
        }

        protected async Task<bool> Push(CommandContext<WebHostCommandOptions> context)
        {
            if (string.IsNullOrEmpty(context.Options.file))
            {
                context.Terminal.WriteError($"[X] File is mandatory");
                return false;
            }

            if (string.IsNullOrEmpty(context.Options.path))
            {
                context.Terminal.WriteError($"[X] Path is mandatory");
                return false;
            }

            if (!File.Exists(context.Options.file))
            {
                context.Terminal.WriteError($"[X] File {context.Options.file} not found");
                return false;
            }

            byte[] fileBytes = File.ReadAllBytes(context.Options.file);

            await context.CommModule.WebHost(context.Options.path, fileBytes, context.Options.powershell, context.Options.description);

            context.Terminal.WriteSuccess($"File {context.Options.file} hosted on {context.Options.path}.");
            return true;
        }

        protected async Task<bool> Show(CommandContext<WebHostCommandOptions> context)
        {
            var list = await context.CommModule.GetWebHosts();
            if (!string.IsNullOrEmpty(context.Options.path))
            {
                if (!list.Any(h => h.Path.ToLower() == context.Options.path.ToLower()))
                {
                    context.Terminal.WriteError($"[X] Host {context.Options.path} not found");
                    return false;
                }
                else
                    list = new List<FileWebHost> { list.First(h => h.Path.ToLower() == context.Options.path.ToLower()) };
            }


            List<TeamServerListener> listeners = null;
            if (!string.IsNullOrEmpty(context.Options.listener))
                listeners = new List<TeamServerListener>() { context.CommModule.GetListeners().First(l => l.Name.ToLower() == context.Options.listener.ToLower()) };
            else
                listeners = context.CommModule.GetListeners().ToList();

            foreach (var listener in listeners)
            {
                var rule = new Rule(listener.Name);
                rule.Style = Style.Parse("cyan");
                context.Terminal.Write(rule);

                if (context.Options.CommandVerb == CommandVerbs.Show)
                {
                    var table = new Table();
                    table.Border(TableBorder.Rounded);
                    table.AddColumn(new TableColumn("Url").LeftAligned());
                    table.AddColumn(new TableColumn("PowerShell").Centered());
                    table.AddColumn(new TableColumn("Description").LeftAligned());
                    //table.AddColumn(new TableColumn("Script").LeftAligned());


                    foreach (var item in list)
                    {
                        var url = listener.EndPoint + "/" + item.Path;
                        table.AddRow(
                            url,
                            item.IsPowershell ? "Yes" : "No",
                            item.Description ?? string.Empty
                            );
                    }
                    table.Expand();
                    context.Terminal.Write(table);
                }

                if (context.Options.CommandVerb == CommandVerbs.Script)
                {
                    foreach (var item in list.Where(i => i.IsPowershell))
                    {
                        var url = listener.EndPoint + "/" + item.Path;
                        context.Terminal.WriteLineMarkup($"[grey]{url}[/]");
                        context.Terminal.WriteLine(ScriptHelper.GeneratePowershellScript(url, listener.Secured));
                        context.Terminal.WriteLine(ScriptHelper.GeneratePowershellScriptB64(url, listener.Secured));
                    }
                }
            }

            return true;
        }

        protected async Task<bool> Log(CommandContext<WebHostCommandOptions> context)
        {
            var list = await context.CommModule.GetWebHostLogs();

            var table = new Table();
            table.Border(TableBorder.Rounded);
            table.AddColumn(new TableColumn("Date").LeftAligned());
            table.AddColumn(new TableColumn("Url").Centered());
            table.AddColumn(new TableColumn("UserAgent").LeftAligned());
            table.AddColumn(new TableColumn("StatusCode").LeftAligned());

            foreach (var item in list)
            {
                if (item == null)
                    continue;



                table.AddRow(
                    item.Date.ToLocalTime().ToString(),
                    item.Url,
                    item.UserAgent,
                    item.StatusCode.ToString()
                    );
            }

            table.Expand();
            context.Terminal.Write(table);

            return true;
        }

        protected async Task<bool> Remove(CommandContext<WebHostCommandOptions> context)
        {
            var list = await context.CommModule.GetWebHosts();
            if (!list.Any(h => h.Path.ToLower() == context.Options.path.ToLower()))
            {
                context.Terminal.WriteError($"[X] Host {context.Options.path} not found");
                return false;
            }

            await context.CommModule.RemoveWebHost(context.Options.path);

            context.Terminal.WriteSuccess($"[*] {context.Options.path} removed from Web Hosting");
            return true;
        }
        protected async Task<bool> Clear(CommandContext<WebHostCommandOptions> context)
        {
            await context.CommModule.ClearWebHosts();
            context.Terminal.WriteSuccess("[*] Web hosting cleared ");
            return true;
        }
    }


}
