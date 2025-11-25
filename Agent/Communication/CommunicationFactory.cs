using Agent.Models;
using Agent.Service;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Communication
{
    public static class CommunicationFactory
    {
        internal static Communicator CreateCommunicator(ConnexionUrl conn)
        {
            if (!conn.IsValid)
                return null;
            switch (conn.Protocol)
            {
                case ConnexionType.Http:
                    return new HttpCommmunicator(conn);
                case ConnexionType.Tcp:
                    case ConnexionType.ReverseTcp:
                    return new TcpCommModule(conn);
                case ConnexionType.NamedPipe:
                case ConnexionType.ReverseNamedPipe:
                    return new PipeCommModule(conn);
            }
            return null;
        }
    }
}
