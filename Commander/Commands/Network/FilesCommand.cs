using Commander.Commands.Network;
using Commander.Communication;
using Commander.Executor;
using Commander.Helper;
using Commander.Terminal;
using Newtonsoft.Json;
using Shared;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands
{
    public class FilesCommandOptions : VerbAwareCommandOptions
    {
        public string id { get; set; }
    }

    public class FilesCommand : VerbAwareCommand<FilesCommandOptions>
    {
        public override string Description => "List files on  the server";
        public override string Name => "file";

        public override ExecutorMode AvaliableIn => ExecutorMode.All;

        public override RootCommand Command => new RootCommand(this.Description)
            {
            new Argument<string>("verb", () => CommandVerbs.Show.Command()).FromAmong(CommandVerbs.Get.Command(), CommandVerbs.Show.Command()),
                new Option<string>(new[] { "--id", "-i" } ,() => null, $"id of the file ({CommandVerbs.Get.Command()})."),
            };

        protected override void RegisterVerbs()
        {
            base.RegisterVerbs();
            Register(CommandVerbs.Get, Get);
            Register(CommandVerbs.Show, Show);
        }

        protected async Task<bool> Show(CommandContext<FilesCommandOptions> context)
        {
            var res = await context.CommModule.GetFiles();
            if (!res.Any())
            {
                context.Terminal.WriteLine("[>] No files on TeamServer!");
                return true;
            }

            var table = new Table();
            table.Border(TableBorder.Rounded);
            // Add some columns
            table.AddColumn(new TableColumn("Id").LeftAligned());
            table.AddColumn(new TableColumn("Agent").LeftAligned());
            table.AddColumn(new TableColumn("File").LeftAligned());

            foreach (var item in res)
            {
                table.AddRow(item.Id, item.Source, item.FileName);
            }

            context.Terminal.Write(table);

            return true;
        }

        protected async Task<bool> Get(CommandContext<FilesCommandOptions> context)
        {
            if (string.IsNullOrEmpty(context.Options.id))
            {
                context.Terminal.WriteError("[X] Id is required to get the file!");
                return false;
            }

            var res = await context.CommModule.GetFile(context.Options.id);
            var fileContent = Convert.FromBase64String(res.Data);

            using (FileStream fs = new FileStream(res.FileName, FileMode.Create, FileAccess.Write))
            {
                fs.Write(fileContent, 0, fileContent.Length);
            }

            context.Terminal.WriteSuccess("[*] File saved locally!");

            return true;
        }

        protected override async Task CallEndPointCommand(CommandContext<FilesCommandOptions> context)
        {
            //override to not send task to Agent
        }

       
    }
}
