using Agent.Communication;
using Agent.Service.Pivoting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Service
{
    public interface IPivotService : IRunningService
    {
        List<PivotServer> Pivots { get; }

        bool AddPivot(ConnexionUrl conn, string serverKey);
        bool RemovePivot(ConnexionUrl conn);

        bool HasPivots();
    }
    public class PivotService : RunningService, IPivotService
    {
        ConcurrentDictionary<string, PivotServer> servers = new ConcurrentDictionary<string, PivotServer>();
        public override string ServiceName => "Pivot";

        public List<PivotServer> Pivots
        {
            get
            {
                return servers.Values.ToList();
            }
        }

        public bool HasPivots()
        {
            return servers.Any();
        }

        public bool AddPivot(ConnexionUrl conn, string serverKey)
        {
            PivotServer server = null;
            switch (conn.Protocol)
            {
                case ConnexionType.Http:
                    {
                        server = new PivotHttpServer(conn, serverKey);
                    }break;
                case ConnexionType.Tcp:
                    {
                        server = new PivotTCPServer(conn, serverKey);
                    }break;
                case ConnexionType.NamedPipe:
                    {
                        server = new PivotPipeServer(conn, serverKey);
                    }
                    break;
                default: return false;
    
            }
            server.Start();

            Thread.Sleep(10);
            if (server.Status == RunningStatus.Running)
            {
                servers.TryAdd(conn.ToString().ToLower(), server);
                return true;
            }
            return false;
        }

        public bool RemovePivot(ConnexionUrl conn)
        {
            var key = conn.ToString().ToLower();
            if (!servers.ContainsKey(key))
                return false;
            var server = servers[key];
            server.Stop();
            return servers.TryRemove(key, out _);
        }



        public override void Stop()
        {
            base.Stop();
            foreach(var key in this.servers.Keys.ToList())
                this.RemovePivot(ConnexionUrl.FromString(key));

        }
    }
}
