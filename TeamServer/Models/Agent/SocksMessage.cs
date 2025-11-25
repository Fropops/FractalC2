using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamServer.Models
{
    public class SocksMessage
    {
        public string Source { get; set; }
        public string Data { get; set; }
        public bool ConnexionState { get; set; }
    }

    
}
