using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shared;
using static Agent.Service.RunningService;

namespace Agent.Service
{
    public interface IRunningService
    {

        string ServiceName { get; }

        RunningStatus Status { get; set; }

        void Start();

        void Stop();
    }
    public abstract class RunningService : IRunningService
    {
        protected virtual JobType? JobType { get { return null; } }
        public abstract string ServiceName { get; }
        public enum RunningStatus
        {
            Running,
            Stoped
        }

        public RunningStatus Status { get; set; } = RunningStatus.Stoped;

        public int MinimumDelay { get; set; } = 10;

        protected CancellationTokenSource _tokenSource;

        public virtual async void Start()
        {
            try
            {
                _tokenSource = new CancellationTokenSource();
                this.Status = RunningStatus.Running;
                if(this.JobType.HasValue)
                {
                    ServiceProvider.GetService<IJobService>().RegisterJob(this.JobType.Value, _tokenSource, this.ServiceName);
                }

                while (!_tokenSource.IsCancellationRequested)
                {
                    await this.Process();
                    await Task.Delay(this.MinimumDelay);
                }
                
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            finally
            {
                this.Status = RunningStatus.Stoped;
            }
        }

        public virtual async void Stop()
        {
            if (this.Status != RunningStatus.Running)
                return;

            this._tokenSource.Cancel();
        }

        public virtual async Task Process()
        {

        }
    }
}
