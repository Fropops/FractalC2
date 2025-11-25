using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamServer.Models
{
    public enum AgentResultStatus
    {
        Queud = 0,
        Running = 1,
        Completed = 2
    }
    public class AgentTaskResult
    {
        public string Id { get; set; }
        public string Result { get; set; }
        public string Objects { get; set; }
        public string Error { get; set; }
        public string Info { get; set; }
        public AgentResultStatus Status { get; set; }

        public List<TaskFileResult> Files { get; set; } = new List<TaskFileResult>();
    }

    public class TaskFileResult
    {
        public string FileId { get; set; }
        public string FileName { get; set; }

        public bool IsDownloaded { get; set; }
    }
}
