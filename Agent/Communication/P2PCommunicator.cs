using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Agent.Communication
{
    public abstract class P2PCommunicator : Communicator
    {
        public CommunicationModuleMode CommunicationMode { get; protected set; }
        public P2PCommunicator(ConnexionUrl connexion) : base(connexion)
        {
            this.CommunicationType = CommunicationType.P2p;
        }

    }
}
