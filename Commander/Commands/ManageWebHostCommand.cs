using Commander.Helper;
using Common.APIModels.WebHost;
using Common.CommandLine.Core;
using Common.Models;
using Spectre.Console;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Commander.Commands
{
    public class ManageWebHostCommandOptions : VerbCommandOption
    {
        [Argument("Verb", "actions available", 0, DefaultValue = "show", AllowedValues = new object[] { "push", "delete", "show", "script", "log", "clear" }, IsRequired = true)]
        public override string verb { get; set; }

        [Option("file", "f", "Path of the local file to push")]
        public string file { get; set; }

        [Option("path", "p", "Hosting path")]
        public string path { get; set; }

        [Option("powershell", "ps", "Specify is the file is a powershell script")]
        public bool powershell { get; set; }

        [Option("description", "d", "Description of the file")]
        public string description { get; set; }

        [Option("listener", "l", "filter on specific listener")]
        public string listener { get; set; }
    }

    [Command("host", "WebHost file on the TeamServer", Category = "Commander")]
    public class ManageWebHostCommand : VerbCommand<CommanderCommandContext, ManageWebHostCommandOptions>
    {
        protected override void RegisterVerbs()
        {
            Register("show", Show);
            Register("push", Push);
            Register("delete", Remove);
            Register("remove", Remove); // Alias just in case
            Register("log", Log);
            Register("clear", Clear);
            Register("script", Show);
        }

        protected async Task<bool> Push(CommanderCommandContext context, ManageWebHostCommandOptions options)
        {
            if (string.IsNullOrEmpty(options.file))
            {
                context.Terminal.WriteError($"[X] File is mandatory");
                return false;
            }

            if (string.IsNullOrEmpty(options.path))
            {
                context.Terminal.WriteError($"[X] Path is mandatory");
                return false;
            }

            if (!File.Exists(options.file))
            {
                context.Terminal.WriteError($"[X] File {options.file} not found");
                return false;
            }

            byte[] fileBytes = File.ReadAllBytes(options.file);

            await context.CommModule.WebHost(options.path, fileBytes, options.powershell, options.description);

            context.Terminal.WriteSuccess($"File {options.file} hosted on {options.path}.");
            return true;
        }

        protected async Task<bool> Show(CommanderCommandContext context, ManageWebHostCommandOptions options)
        {
            var list = await context.CommModule.GetWebHosts();
            if (!string.IsNullOrEmpty(options.path))
            {
                if (!list.Any(h => h.Path.ToLower() == options.path.ToLower()))
                {
                    context.Terminal.WriteError($"[X] Host {options.path} not found");
                    return false;
                }
                else
                    list = new List<FileWebHost> { list.First(h => h.Path.ToLower() == options.path.ToLower()) };
            }


            List<TeamServerListener> listeners = null;
            if (!string.IsNullOrEmpty(options.listener))
                listeners = new List<TeamServerListener>() { context.CommModule.GetListeners().First(l => l.Name.ToLower() == options.listener.ToLower()) };
            else
                listeners = context.CommModule.GetListeners().ToList();

            foreach (var listener in listeners)
            {
                var rule = new Rule(listener.Name);
                rule.Style = Style.Parse("cyan");
                context.Terminal.Write(rule);

                if (options.verb == "show")
                {
                    var table = new Table();
                    table.Border(TableBorder.Rounded);
                    table.AddColumn(new TableColumn("Url").LeftAligned());
                    table.AddColumn(new TableColumn("PowerShell").Centered());
                    table.AddColumn(new TableColumn("Description").LeftAligned());

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

                if (options.verb == "script")
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

        protected async Task<bool> Log(CommanderCommandContext context, ManageWebHostCommandOptions options)
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

        protected async Task<bool> Remove(CommanderCommandContext context, ManageWebHostCommandOptions options)
        {
            var list = await context.CommModule.GetWebHosts();
            if (!list.Any(h => h.Path.ToLower() == options.path.ToLower()))
            {
                context.Terminal.WriteError($"[X] Host {options.path} not found");
                return false;
            }

            await context.CommModule.RemoveWebHost(options.path);

            context.Terminal.WriteSuccess($"[*] {options.path} removed from Web Hosting");
            return true;
        }
        protected async Task<bool> Clear(CommanderCommandContext context, ManageWebHostCommandOptions options)
        {
            await context.CommModule.ClearWebHosts();
            context.Terminal.WriteSuccess("[*] Web hosting cleared ");
            return true;
        }
    }
}
