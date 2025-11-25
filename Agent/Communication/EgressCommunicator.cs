using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shared;

namespace Agent.Communication
{
    internal abstract class EgressCommunicator : Communicator
    {
        public override event Func<NetFrame, Task> FrameReceived;
        public override event Action OnException;
        public EgressCommunicator(ConnexionUrl connexion) : base(connexion)
        {
            this.CommunicationType = CommunicationType.Egress;
        }

        public override async Task Start()
        {

        }

        public override async Task Run()
        {
            this.IsRunning = true;
            while (!_tokenSource.IsCancellationRequested)
            {
                try
                {
                    await this.DoCheckIn();
                }
                catch (Exception ex)
                {
#if DEBUG
                    Console.WriteLine(ex.ToString());
#endif
                }

                try
                {
                    Task.Delay(this.GetDelay()).Wait();
                }
                catch (TaskCanceledException ex)
                {
                    //ignore
                }
            }

            this.IsRunning = false;
        }

        public async Task DoCheckIn()
        {
            var frames = await CheckIn(this.NetworkeService.GetFrames(String.Empty));
            if (frames != null)
                frames.ForEach(frame => this.FrameReceived?.Invoke(frame));
        }

        protected abstract Task<List<NetFrame>> CheckIn(List<NetFrame> frames);

        public static TimeSpan CalculateSleepTime(int interval, int jitter)
        {
            var diff = (int)Math.Round((double)interval / 100 * jitter);

            var min = interval - diff;
            var max = interval + diff;

            var rand = new Random();
            return new TimeSpan(0, 0, rand.Next(min, max));
        }

        protected virtual TimeSpan GetDelay()
        {
            return CalculateSleepTime(this.Agent.MetaData.SleepInterval, this.Agent.MetaData.SleepJitter);
        }

        public override async Task SendFrame(NetFrame frame)
        {
            this.NetworkeService.EnqueueFrame(frame);
        }

    }
}
