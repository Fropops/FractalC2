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

        public static implicit operator TeamServerImplant(Implant implant)
        {
            return new TeamServerImplant
            {
                Id = implant.Id,
                Name = implant.Name,
                Data =implant.Data,
                Config = implant.Config,
                Listener = implant.Listener,
            };

        }

        public static implicit operator Implant(TeamServerImplant dao)
        {
            if (dao == null) return null;

            return new Implant(dao.Id)
            {
                Name = dao.Name,
                Data = dao.Data,
                Config = dao.Config,
                Listener = dao.Listener,
            };
        }
    }
}
