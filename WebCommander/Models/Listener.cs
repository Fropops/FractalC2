namespace WebCommander.Models
{
    public class Listener
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int BindPort { get; set; }
        public bool Secured { get; set; }
        public string Ip { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is Listener listener && Id == listener.Id;
        }

        public override int GetHashCode()
        {
            return Id?.GetHashCode() ?? 0;
        }
    }
}
