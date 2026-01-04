using System;
using Common.Models;
using Common.Payload;
using Newtonsoft.Json;
using TeamServer.Database;

namespace TeamServer.Models
{
    public class Implant
    {
        public Implant(string id)
        {
                this.Id = id;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Data { get; set; }
        public ImplantConfig Config { get; set; }
        public string Listener { get; set; }

        public bool IsDeleted { get; set; }
    }
}
