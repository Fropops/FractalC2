using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinarySerializer;
using Common.Payload;

namespace Common.Models
{
    public class TeamServerImplant
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Data { get; set; }
        public PayloadType Type { get; set; }
        public string Extension { get; set; }

        public string Listener { get; set; }
        public string Endpoint { get; set; }
    }
}
