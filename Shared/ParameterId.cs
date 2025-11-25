using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public enum ParameterId : byte
    {
        Command = 0x00,
        File = 0x01,
        Path = 0x02,
        Payload = 0x03,
        Verb = 0x04,
        Id = 0x05,
        User = 0x06,
        Domain = 0x07,
        Password = 0x08,
        Name = 0x09,
        Parameters = 0x10,
        Output = 0x11,
        Bind = 0x12,
        Delay = 0x13,
        Jitter = 0x14,
        Port = 0x15,
        Target = 0x16,
        Service = 0x17,
        Recursive = 0x18,
        Key = 0x19,
        Value = 0x20,
    }
}
