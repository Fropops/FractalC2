using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class TeamServerListener
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int BindPort { get; set; }
        public bool Secured { get; set; }

        public string Ip { get; set; }

        public string EndPoint
        {
            get { return (Secured ? "https" : "http") + "://" + Ip + ":" + BindPort.ToString(); }
        }
    }
}
