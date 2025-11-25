using Agent.Models;
using Agent.Service;
using Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Service
{
    public interface INetworkService
    {
        void EnqueueFrame(NetFrame frame);

        List<NetFrame> GetFrames(string destination, NetFrameType frameType);

        List<NetFrame> GetFrames(string destination);
    }

    public class NetworkService : INetworkService
    {
        public ConcurrentDictionary<string, ConcurrentDictionary<NetFrameType, ConcurrentQueue<NetFrame>>> Frames = new ConcurrentDictionary<string, ConcurrentDictionary<NetFrameType, ConcurrentQueue<NetFrame>>>();

        public NetworkService()
        {
        }

        public void EnqueueFrame(NetFrame frame)
        {
            ConcurrentDictionary<NetFrameType, ConcurrentQueue<NetFrame>> destDico = null;
            if (!Frames.ContainsKey(frame.Destination))
            {
                destDico = new ConcurrentDictionary<NetFrameType, ConcurrentQueue<NetFrame>>();
                Frames.TryAdd(frame.Destination, destDico);
            }
            else
                destDico = Frames[frame.Destination];

            ConcurrentQueue<NetFrame> queue = null;
            if (!destDico.ContainsKey(frame.FrameType))
            {
                queue = new ConcurrentQueue<NetFrame>();
                destDico.TryAdd(frame.FrameType, queue);
            }
            else
                queue = destDico[frame.FrameType];

            queue.Enqueue(frame);
        }

        public List<NetFrame> GetFrames(string destination, NetFrameType frameType)
        {
            if (!Frames.ContainsKey(destination))
                return new List<NetFrame>();

            var dico = Frames[destination];
            if(!dico.ContainsKey(frameType))
                return new List<NetFrame>();

            var queue = dico[frameType];

            List<NetFrame> frames = new List<NetFrame>();
            while(queue.TryDequeue(out var frame))
                frames.Add(frame);

            return frames;
        }

        public List<NetFrame> GetFrames(string destination)
        {
            if (!Frames.ContainsKey(destination))
                return new List<NetFrame>();

            var dico = Frames[destination];

            List<NetFrame> frames = new List<NetFrame>();
            foreach (var frameType in dico.Keys)
            {
                var queue = dico[frameType];

                while (queue.TryDequeue(out var frame))
                    frames.Add(frame);
            }

            return frames;
        }
    }
}
