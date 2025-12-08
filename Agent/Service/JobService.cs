using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shared;

namespace Agent.Service
{
    internal interface IJobService
    {
        Job RegisterJob(JobType type, int processId, string name, string taskId = null);

        Job RegisterJob(JobType type, CancellationTokenSource token, string name, string taskId = null);
        bool RemoveJob(int id);
        Job GetJob(int id);
        List<Job> GetJobs();
    }
    internal class JobService : IJobService
    {
        private int NextId = 0;
        private ConcurrentDictionary<int, Job> Jobs = new ConcurrentDictionary<int, Job>();

        public Job RegisterJob(JobType type, int processId, string name, string taskId = null)
        {
            var job = new Job(type, this.NextId++, processId, name, taskId);
            this.Jobs.AddOrUpdate(job.Id, job, (key, value) => value);
            return job;
        }

        public Job RegisterJob(JobType type, CancellationTokenSource token, string name, string taskId = null)
        {
            var job = new Job(type, this.NextId++, token, name, taskId);
            this.Jobs.AddOrUpdate(job.Id, job, (key, value) => value);
            return job;
        }

        public bool RemoveJob(int id)
        {
            return this.Jobs.TryRemove(id, out Job _);
        }

        public Job GetJob(int id)
        {
            this.Jobs.TryGetValue(id, out Job job);
            return job;
        }

        public List<Job> GetJobs()
        {
            return this.Jobs.Values.ToList();
        }
    }
}
