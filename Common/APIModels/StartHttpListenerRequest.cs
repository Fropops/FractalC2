using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.APIModels
{
    public class StartHttpListenerRequest
    {
        public string Name { get; set; }

        public string Ip { get; set; }
        public int BindPort { get; set; }

        public bool Secured { get; set; }
    }
}
