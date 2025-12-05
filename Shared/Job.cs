using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinarySerializer;

namespace Shared
{
    public enum JobType : byte
    {
        ForkAndRun = 0,
        InlineAssembly,
        Shell,
        KeyLog
    }

    public class Job
    {
        [FieldOrder(0)]
        public JobType JobType { get; set; }
        [FieldOrder(1)]
        public string Name { get; set; }
        [FieldOrder(2)]
        public int Id { get; set; }
        [FieldOrder(3)]
        public string TaskId { get; set; }
        [FieldOrder(4)]
        public int? ProcessId { get; set; }

        public CancellationTokenSource CancellationToken { get; set; }


        public Job(JobType type, int id, int processId, string name, string taskId)
        {
            this.JobType = type;
            this.Id = id;
            this.Name = name;
            this.ProcessId = processId;
            this.TaskId = taskId;
        }

        public Job(JobType type, int id, CancellationTokenSource token, string name, string taskId)
        {
            this.JobType = type;
            this.Id = id;
            this.Name = name;
            this.CancellationToken = token;
            this.TaskId = taskId;
        }

        public Job()
        {

        }
    }
}
