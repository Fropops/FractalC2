using Common.Payload;

namespace Common.APIModels
{
    public class APIImplant
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Data { get; set; }
        public ImplantConfig Config { get; set; }
        public string Listener { get; set; }
    }
}
