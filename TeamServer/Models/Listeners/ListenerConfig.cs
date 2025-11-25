using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamServer
{
    public class ListenerConfig
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public int BindPort { get; set; }
        public bool Secured { get; set; }
    }
}
