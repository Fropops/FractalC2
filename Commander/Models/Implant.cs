using System;
using Common.Payload;

namespace Commander.Models
{
    public class Implant
    {
        public string Id { get; set; }
        public string Data { get; set; }
        public ImplantConfig Config { get; set; }
        public string Listener { get; set; }
    }
}
