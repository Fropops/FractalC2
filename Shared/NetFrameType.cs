using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public enum NetFrameType : byte
    {
        CheckIn = 0x00,
        Task = 0x01,
        TaskResult = 0x02,
        Link = 0x03,
        Unlink = 0x04,
        LinkRelay = 0x05,
        Socks = 0x06,
        ReversePortForward = 0x07,

    }
}
