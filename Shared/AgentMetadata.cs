using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BinarySerializer;

namespace Shared
{
    public enum IntegrityLevel : byte
    {
        Medium = 0x00,
        High = 0x01,
        System = 0x02
    }

    public class AgentMetadata
    {
        [FieldOrder(0)]
        public string Id { get; set; }
        [FieldOrder(1)]
        public string ImplantId { get; set; }
        [FieldOrder(2)]
        public string Hostname { get; set; }
        [FieldOrder(3)]
        public string UserName { get; set; }
        [FieldOrder(4)]
        public string ProcessName { get; set; }
        [FieldOrder(5)]
        public int ProcessId { get; set; }
        [FieldOrder(6)]
        public IntegrityLevel Integrity { get; set; }
        [FieldOrder(7)]
        public string Architecture { get; set; }
        [FieldOrder(8)]
        public string EndPoint { get; set; }
        [FieldOrder(9)]
        public string Version { get; set; }

        [FieldOrder(10)]
        public byte[] Address { get; set; }

        [FieldOrder(11)]
        public int SleepInterval { get; set; }
        [FieldOrder(12)]
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
                if(this.HasElevatePrivilege())
                    desc += "*";
                desc += "@" + Hostname;
                return desc;
            }
        }
    }
}
