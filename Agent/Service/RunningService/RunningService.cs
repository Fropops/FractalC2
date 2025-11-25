using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
