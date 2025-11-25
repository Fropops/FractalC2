using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public enum CommandVerbs : byte
    {
        Start = 0x00,
        Stop = 0x01,
        Show = 0x03,
        Kill = 0x04,
        Push = 0x05,
        Remove = 0x06,
        Log = 0x07,
        Script = 0x08,
        Clear = 0x09,
        Get = 0x10,
        Add = 0x11,
    }
}
