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
        public static void Log(string message)
        {
            if (!Active)
                return;

            System.IO.File.AppendAllText(FileName, DateTime.Now.ToString() + " => " + message + Environment.NewLine);
        }
    }
}
