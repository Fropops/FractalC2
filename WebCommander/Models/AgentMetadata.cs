namespace TeamServer.UI.Models
{
    public enum IntegrityLevel : byte
    {
        Medium = 0x00,
        High = 0x01,
        System = 0x02
    }

    public class AgentMetadata
    {
        public string Id { get; set; }
        public string ImplantId { get; set; }
        public string Hostname { get; set; }
        public string UserName { get; set; }
        public string ProcessName { get; set; }
        public int ProcessId { get; set; }
        public IntegrityLevel Integrity { get; set; }
        public string Architecture { get; set; }
        public string EndPoint { get; set; }
        public string Version { get; set; }
        public byte[] Address { get; set; }
        public int SleepInterval { get; set; }
        public int SleepJitter { get; set; }

        public string Sleep
        {
            get { return $"{this.SleepInterval}s - {this.SleepJitter}%"; }
        }

        public bool HasElevatePrivilege()
        {
            return this.Integrity == IntegrityLevel.System || this.Integrity == IntegrityLevel.High;
        }

        public string Desc
        {
            get
            {
                string desc = UserName;
                if (this.HasElevatePrivilege())
                    desc += "*";
                desc += "@" + Hostname;
                return desc;
            }
        }
    }
}
