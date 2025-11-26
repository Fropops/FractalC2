namespace WebCommander.Models
{
    public class StartHttpListenerRequest
    {
        public string Name { get; set; }
        public string Ip { get; set; }
        public int BindPort { get; set; }
        public bool Secured { get; set; }
    }
}
