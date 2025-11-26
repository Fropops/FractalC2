using Common.Payload;

namespace TeamServer.Models.Implant
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
        public PayloadType Type { get; set; }
        public string Extension { get; set; }

        public string Listener { get; set; }
        public string Endpoint { get; set; }

        public bool IsDeleted { get; set; }
    }
}
