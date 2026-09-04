using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamServer
{
    public static class Logger
    {
        public static bool Active { get; set; } = true;

        public static string FileName { get; set; } = "log.log";

        private static readonly object _lock = new object();

        public static void Log(string message)
        {
            if (!Active)
                return;

            lock (_lock)
            {
                System.IO.File.AppendAllText(FileName, DateTime.Now.ToString() + " => " + message + Environment.NewLine);
            }
        }
    }
}
